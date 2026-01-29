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

import { getCardTargetActor, getCardTargetActorDescription } from "../../apiv2/actors/actors.mjs";
import { send } from "../../apiv2/actors/messages.mjs";
import { getProps, propCardTargetActor, propDecimal } from "../../apiv2/actors/properties.mjs";
import { getPos } from "../../apiv2/transform/position-get.mjs";
import { getForward } from "../../apiv2/transform/rotation-get.mjs";

export const PROPS = [
  propCardTargetActor("CardTargetActor", {
    label: "Set on who?"
  }),
  propCardTargetActor("Destination", {
    label: "To what destination?"
  }),
  propDecimal('Speed', 5)
];

/**
 * @param {GActionMessage} actionMessage 
 */
export function onAction(actionMessage) {
  const target = getCardTargetActor("CardTargetActor", actionMessage);
  if (!target) {
    return;
  }
  const destActor = getCardTargetActor("Destination", actionMessage);
  if (destActor) {
    send(target, 'SetDestination', {
      pos: getPos(destActor),
      forward: getForward(1, destActor),
      speed: getProps().Speed
    });
  }
}

export function getCardStatus() {
  return {
    description: `Set auto-pilot destination of <color=yellow>${getCardTargetActorDescription('CardTargetActor')}</color> to <color=green>${getCardTargetActorDescription('Destination')}</color>.`
  }
}