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

import { getActorGroupDescription, getActorsInGroup, getClosestActor, myself } from "../../apiv2/actors/actors.mjs";
import { getDisplayName } from "../../apiv2/actors/attributes.mjs";
import { clone } from "../../apiv2/actors/cloning.mjs";
import { getProps, propActor, propActorGroup, propDecimal, propSound } from "../../apiv2/actors/properties.mjs";
import { vec3 } from "../../apiv2/misc/math.mjs";
import { push } from "../../apiv2/physics/velocity.mjs";
import { setVarPlease } from "../../apiv2/remote/remote.mjs";
import { playSound, Sounds } from "../../apiv2/sfx/sfx.mjs";
import { selfToWorldPos } from "../../apiv2/transform/position-get.mjs";
import { getForward, getRot } from "../../apiv2/transform/rotation-get.mjs";
import { lookAt } from "../../apiv2/transform/rotation-set.mjs";

export const PROPS = [
  propActor("Projectile", "builtin:LaserBolt", {
    pickerPrompt: "What should I shoot?",
    allowOffstageActors: true
  }),
  propActorGroup("Targets", "@ANY", {
    label: "Who should I fire at?"
  }),
  propSound("Sound", Sounds.LASER),
  propDecimal("Velocity", 15),
  propDecimal("Range", 20),
  propDecimal("OffsetY", 1),
  propDecimal("OffsetZ", 2)
]

/**
 * @param {GActionMessage} actionMessage
 */
export function onAction(actionMessage) {
  // Figure out who we should target.
  const target = getClosestActor(getActorsInGroup(getProps().Targets, getProps().Range));
  if (!target) return;
  // Turn to face the target.
  lookAt(target, true);
  // Calculate the position where we should spawn the projectile.
  const spawnPos = selfToWorldPos(vec3(0, getProps().OffsetY, getProps().OffsetZ));
  const proj = clone(getProps().Projectile, spawnPos, getRot());
  // Set ourselves as the projectile's owner (for scoring).
  setVarPlease(proj, "owner", myself());
  // Push the projectile along our forward direction.
  push(proj, getForward(getProps().Velocity));
  // Play sound.
  if (getProps().Sound) {
    playSound(getProps().Sound);
  }
}

export function onGetActionDescription() {
  return "Shoot (group)";
}

export function getCardStatus() {
  return {
    description: `Fire projectile <color=white>${getDisplayName(getProps().Projectile)}</color> at <color=green>${getActorGroupDescription(getProps().Targets)}</color> with velocity <color=yellow>${getProps().Velocity.toFixed(1)}</color>`
  }
}