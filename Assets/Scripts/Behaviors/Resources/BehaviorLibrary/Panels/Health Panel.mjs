/*
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

import { myself } from "../../apiv2/actors/actors.mjs";
import { getDisplayName, setVar } from "../../apiv2/actors/attributes.mjs";
import { callActionDeck } from "../../apiv2/actors/messages.mjs";
import { getCard, getMem, getTemp } from "../../apiv2/actors/memory.mjs";
import { cooldown, sendToAll } from "../../apiv2/actors/messages.mjs";
import { propBoolean, propDecimal, propDeck, propNumber, requireTrue } from "../../apiv2/actors/properties.mjs";
import { legacyApi } from "../../apiv2/apiv2.mjs";
import { clamp } from "../../apiv2/misc/math.mjs";
import { getTime } from "../../apiv2/misc/time.mjs";
import { isPlayerControllable } from "../../apiv2/player_controls/controls.mjs";
import { hide, show } from "../../apiv2/rendering/visibility.mjs";
import { assertNumber } from "../../util.mjs";
import { spin } from "../../apiv2/transform/rotation-set.mjs";
import { getProps } from "../../apiv2/actors/properties.mjs";

export const PROPS = [
  propNumber("StartingHealth", 3),
  // TEMP propNumber("DamageCooldown", 1),
  propDeck('damageDeck', 'Action', {
    label: 'When Damaged:',
    deckOptions: {
      defaultCardURIs: ['builtin:Change Tint']
    }
  }),
  propDeck('preDeathDeck', 'Action', {
    label: 'When About to Die (before delay):'
  }),
  propDeck('deathDeck', 'Action', {
    label: 'At Death (after delay):',
    deckOptions: {
      defaultCardURIs: ['builtin:Destroy Self Action Card']
    }
  }),
  propBoolean("overrideDeathDelay", false, {
    label: "Override death delay"
  }),
  propDecimal("deathDelay", 0.2, {
    requires: [requireTrue("overrideDeathDelay")],
    label: "Death delay"
  }),
  propBoolean("hideWhileDying", true, {
    label: "Hide when dying/dead"
  })
]

export function onInit() {
  getMem().health = getProps().StartingHealth;
  // Same as reviving.
  onRevive();
  updateVars();
}

function updateVars() {
  // Publish these vars for the benefit of other cards/actors:
  setVar("isDead", !!getMem().isDead);
  setVar("health", getMem().health || getProps().StartingHealth);
  setVar("startingHealth", getProps().StartingHealth);
}

export function onTick() {
  // If we find out that we died here, as opposed to onDamage, it's because we
  // died as a result of some behavior simply deducting health instead of using
  // a Damage message, so the "event" is a dummy event.

  /** @type {GEvent} */
  const event = { actor: myself() };
  checkDeath(event);

  // Publish attributes:
  updateVars();

  // If it's time to run the death actions, do it now.
  maybeRunDeathActions();
}

/**
 * Damage message handler.
 * When we receive a Damage message, we deduct health from the actor.
 * @param {GDamageMessage} damageMessage
 */
export function onDamage(damageMessage) {
  // If we were just revived, don't take damage yet. This avoids race conditions when you
  // get revived and teleported away from damage and still have a stray damage message
  // from the situation you were in.
  if (getTime() - (getTemp().reviveTime || 0) < 0.5) {
    return;
  }

  // If we are already dead, no further damage can be taken.
  if (getMem().isDead) {
    return;
  }

  // Deduct the damage from health. Don't go below 0 or above maximum.
  // (Remember the damage can be negative to mean "heal").
  let amount = 1;
  if (damageMessage.amount !== undefined) {
    assertNumber(damageMessage.amount, "damageMessage.amount");
    amount = damageMessage.amount;
  }
  // event.actor is the "event causer", so we set it to the causer of the damage.
  let event = { actor: damageMessage.causer || myself() };

  getMem().health = clamp(getMem().health - amount, 0, getProps().StartingHealth);
  // Did we die?
  checkDeath(event);
  // Call any on-damage actions that were requested, if this in fact damage (amount > 0)
  if (amount > 0) {
    callActionDeck("damageDeck", { event: event });
    // Do the engine-provided damage effect.
    legacyApi().sendMessageToUnity("Damaged");
  }
  // Don't take damage for a while
  // TEMP cooldown(getProps().DamageCooldown);
  cooldown(0.5);
}

/**
 * Revive message handler.
 * When we receive a Revive message, we bring the actor back to life at full health.
 */
export function onRevive() {
  const wasDead = getMem().isDead;
  getMem().health = getProps().StartingHealth;
  getMem().isDead = false;
  if (wasDead) {
    legacyApi().sendMessageToUnity("Respawned");
  }
  if (getProps().hideWhileDying) {
    show();
  }
  getTemp().reviveTime = getTime();
  delete getCard().death;
}

/** @param {GEvent} event The event that may have caused our death. */
function checkDeath(event) {
  if (getMem().isDead || typeof getMem().health !== 'number' || getMem().health > 0) {
    // Not dead, or already dead. In any case, nothing new.
    return;
  }
  // We're dying! Oh no!

  // Run the "when about to die" deck now.
  callActionDeck("preDeathDeck", { event: event });

  getMem().isDead = true;
  // Do the engine-provided death effect.
  legacyApi().sendMessageToUnity("Died");
  if (getProps().hideWhileDying) {
    hide();
  }
  // Note that we don't, by default, do anything special on death --
  // we leave that up to the action cards.

  // Give a bit of a delay so the animations can play for proper
  // dramatic effect...
  const deathDelay = getProps().overrideDeathDelay ? getProps().deathDelay : (isPlayerControllable() ? 3 : 0);
  getCard().death = {
    stage: 1,
    time: getTime() + deathDelay,
    event: event
  };
}

function maybeRunDeathActions() {
  if (!getCard().death || !getCard().death.stage) return;
  switch (getCard().death.stage) {
    case 0:
      // Not dying.
      break;
    case 1:
      // Waiting for the death timer.
      if (getTime() < getCard().death.time) return;
      // Fire the death message (in the next frame we will handle the death actions).
      /** @type {GDeathMessage} */
      let deathMessage = { actor: myself() };
      sendToAll("Death", deathMessage);
      // Don't run actions yet, to give time for the message receivers to do something
      // before the actor actually dies.
      getCard().death.stage = 2;
      break;
    case 2:
      // Time to run the death action cards.
      callActionDeck("deathDeck", { event: getCard().death.event });
      // TODO: what if one of the cards is Destroy Self and another card is some
      // effect like spin, etc, in that case we'd want to delay the destruction
      // until the spin effect has ran for a bit? Maybe not?
      delete getCard().death;
      break;
    default:
      throw new Error("Invalid getCard().death.stage " + getCard().death.stage);
  }
}

export function getDescription() {
  return `${getDisplayName()} will start with ${getProps().StartingHealth} HP`;
}