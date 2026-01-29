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

import { getAttrib } from "../../apiv2/actors/attributes.mjs";
import { propDecimal } from "../../apiv2/actors/properties.mjs";
import { vec3zero } from "../../apiv2/misc/math.mjs";
import { getWorldThrottle, isSprinting } from "../../apiv2/player_controls/controls.mjs";
import { moveGlobal } from "../../apiv2/transform/position-set.mjs";

export const PROPS = [
  propDecimal("Speed", 8),
  propDecimal("SprintSpeed", 12),
];

export function onActiveTick() {
  if (getAttrib("isDead", false) || getAttrib("ControlsLocked", false)) {
    moveGlobal(vec3zero());
    return;
  }
  const velocity = getWorldThrottle();
  velocity.multiplyScalar(isSprinting() ? getProps().SprintSpeed : getProps().Speed);
  moveGlobal(velocity);
}

export function getCardStatus() {
  return {
    description: `(Player) Move based on WASD, speed <color=green>${getProps().Speed.toFixed(1)}</color>, sprint <color=green>${getProps().SprintSpeed.toFixed(1)}</color>.`
  }
}