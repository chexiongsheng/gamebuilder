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

import { getActorGroupDescription, getActorsInGroup, isActorInGroup, isOnstage } from "../../apiv2/actors/actors.mjs";
import { getCard } from "../../apiv2/actors/memory.mjs";
import { getProps, propActorGroup } from "../../apiv2/actors/properties.mjs";

export const PROPS = [
  propActorGroup("targets", "@TAG:enemy", {
    label: "Kill what actors:",
    pickerPrompt: "What actors must be killed?"
  }),
]

/** @return {GEvent|undefined} The event that fired, if any. */
export function onCheck() {
  if (getCard().triggeredEvent) {
    const e = getCard().triggeredEvent;
    delete getCard().triggeredEvent;
    return e;
  }
}

/** @param {GDeathMessage} deathMessage */
export function onDeath(deathMessage) {
  if (!isActorInGroup(deathMessage.actor, getProps().targets)) {
    return;
  }
  for (let actor of getActorsInGroup(getProps().targets)) {
    if (actor !== deathMessage.actor && isOnstage(actor)) {
      // Found at least one alive and onstage, so don't fire yet.
      return;
    }
  }
  // Do not pass an actor here, since that doesn't entirely make sense.
  /** @type {GEvent} */
  getCard().triggeredEvent = {};
}

export function onResetGame() {
  delete getCard().triggeredEvent;
}

export function getCardStatus() {
  return {
    description: `When the last of <color=yellow>${getActorGroupDescription(getProps().targets)}</color> is killed.`
  }
}