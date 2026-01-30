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

import { propBoolean, propDecimal, requireTrue } from "../../apiv2/actors/properties.mjs";
import { isPlayerControllable } from "../../apiv2/player_controls/controls.mjs";
import { lookTowardDir } from "../../apiv2/transform/rotation-set.mjs";;
import { lookDir } from "../../apiv2/transform/rotation-set.mjs";
import { getAimDirection } from "../../apiv2/player_controls/aiming.mjs";
import { getRight } from "../../apiv2/transform/rotation-get.mjs";
import { degToRad } from "../../apiv2/misc/math.mjs";

// Player Controls: Basic WASD.

export const PROPS = [
  propBoolean("Gradual", false, {
    label: "Turn gradually"
  }),
  propDecimal("TurnSpeed", 2, {
    label: "Turn speed",
    requires: [requireTrue("Gradual")]
  }),
  propBoolean("TiltUpDown", false, {
    label: "Tilt character up/down"
  }),
  //propDecimal("PitchOffset", 0),
];

export function onActiveTick() {
  if (!isPlayerControllable()) return;

  let desiredDir = getAimDirection();
  //if (Math.abs(getProps().PitchOffset) > 0.01) {
  //  desiredDir.applyAxisAngle(getRight(), degToRad(getProps().PitchOffset));
  //}

  if (getProps().Gradual) {
    lookTowardDir(desiredDir, getProps().TurnSpeed, !getProps().TiltUpDown);
  } else {
    lookDir(getAimDirection(), !getProps().TiltUpDown);
  }
}