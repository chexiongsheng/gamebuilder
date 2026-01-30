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

import { sendToAll, sendToAllDelayed, sendToMany, sendToManyDelayed } from "../../apiv2/actors/messages.mjs";
import { getProps, propBoolean, propDecimal, propString, requireTrue } from "../../apiv2/actors/properties.mjs";
import { overlapSphere } from "../../apiv2/physics/casting.mjs";
import { getPos } from "../../apiv2/transform/position-get.mjs";

export const PROPS = [
  propString("Message", "Ping"),
  propBoolean("LimitRange", false, {
    label: "Limit range"
  }),
  propDecimal("Range", 20, {
    requires: [requireTrue("LimitRange")]
  }),
  propDecimal("Delay", 0)
];

/**
 * @param {GActionMessage} actionMessage 
 */
export function onAction(actionMessage) {
  if (getProps().LimitRange) {
    const targets = overlapSphere(getPos(), getProps().Range);
    if (getProps().Delay > 0) {
      sendToManyDelayed(getProps().Delay, targets, getProps().Message);
    } else {
      sendToMany(targets, getProps().Message);
    }
  } else {
    if (getProps().Delay > 0) {
      sendToAllDelayed(getProps().Delay, getProps().Message);
    } else {
      sendToAll(getProps().Message);
    }
  }
}

export function getCardStatus() {
  let delay = '';
  let range = '';
  if (getProps().Delay > 0) {
    delay = ` with delay <color=green>${getProps().Delay.toFixed(1)}s</color>`;
  }
  if (getProps().LimitRange) {
    range = ` with range <color=orange>${getProps().Range.toFixed(1)}</color>`;
  }
  return {
    description: `Broadcasts message <color=yellow>${getProps().Message}</color>${delay}${range}`
  }
}