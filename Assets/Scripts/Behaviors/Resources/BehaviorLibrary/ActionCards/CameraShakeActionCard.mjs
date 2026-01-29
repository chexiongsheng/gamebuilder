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
import { requestCameraOffset } from "../../apiv2/actors/camera_light.mjs";
import { getCard } from "../../apiv2/actors/memory.mjs";
import { getProps, propDecimal } from "../../apiv2/actors/properties.mjs";
import { interp, vec3 } from "../../apiv2/misc/math.mjs";
import { getTime } from "../../apiv2/misc/time.mjs";
import { logError } from "../../apiv2/misc/utility.mjs";
import { isPlayerControllable } from "../../apiv2/player_controls/controls.mjs";

export const PROPS = [
  propDecimal('Amplitude', 0.2),
  propDecimal('Duration', 0.1),
];

/**
 * @param {GActionMessage} actionMessage
 */
export function onAction(actionMessage) {
  if (!isPlayerControllable()) {
    logError("Camera Shake only works on a player actor.");
    return;
  }
  getCard().shake = {
    actor: myself(),
    startTime: getTime()
  };
}

export function getCardErrorMessage() {
  if (!isPlayerControllable()) {
    return "*** Card only works on a player actor!";
  }
}

export function onTick() {
  if (!getCard().shake) return;
  const t = getTime() - getCard().shake.startTime;
  if (t > getProps().Duration) {
    stopShaking();
    return;
  }
  // Decay factor starts at 1 and linearly decreases to 0 as the effect progresses.
  const decay = interp(0, 1, getProps().Duration, 0, t);

  const randomX = -0.5 + Math.random() * 2;
  const randomY = -0.5 + Math.random() * 2;

  // Request that offset.
  requestCameraOffset(vec3(randomX * getProps().Amplitude * decay,
    randomY * getProps().Amplitude * decay, 0), getCard().shake.actor);
}

function stopShaking() {
  delete getCard().shake;
}

export function onResetGame() {
  stopShaking();
}
