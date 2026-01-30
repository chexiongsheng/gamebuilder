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
import { getCard } from "../../apiv2/actors/memory.mjs";
import { propDecimal } from "../../apiv2/actors/properties.mjs";
import { degToRad } from "../../apiv2/misc/math.mjs";
import { deltaTime } from "../../apiv2/misc/time.mjs";
import { getThrottle, lookTowardDir, moveForward } from "../../apiv2/player_controls/controls.mjs";
import { getAimDirection } from "../../apiv2/player_controls/aiming.mjs";
import { enableGravity, enableKeepUpright } from "../../apiv2/physics/attributes.mjs";
import { getRight } from "../../apiv2/transform/rotation-get.mjs";

// Player Controls: Hovercraft.

export const PROPS = [
  propDecimal("Accel", 8),
  propDecimal("MaxSpeed", 16),
  propDecimal("BrakeAccel", 32),
  propDecimal("TurnSpeed", 2),
  propDecimal("PitchOffset", 20),
  propDecimal("MaxPitch", 20),
];

export function onControl() {
  const COAST_ACC = 2;

  getCard().speed = getCard().speed || 0;

  enableGravity(false);
  enableKeepUpright(false);

  if (getAttrib("isDead", false)) {
    return;
  }
  const throttle = getThrottle();

  if (throttle.z > 0) {
    getCard().speed += deltaTime() * getProps().Accel;
  } else {
    const brakeAccel = throttle.z < -0.1 ? getProps().BrakeAccel : COAST_ACC;
    getCard().speed = getCard().speed > 0 ?
      Math.max(0, getCard().speed - brakeAccel * deltaTime()) :
      Math.min(0, getCard().speed + brakeAccel * deltaTime());
  }
  getCard().speed = Math.min(Math.max(getCard().speed, -getProps().MaxSpeed), getProps().MaxSpeed);

  const desiredDir = getAimDirection().clone();

  // Offset the pitch so that it's more comfortable to control.
  // We don't HAVE to do it, but users probably expect to look at a ship
  // from above and have it fly perfectly level.
  desiredDir.applyAxisAngle(getRight(), -degToRad(getProps().PitchOffset));

  lookTowardDir(desiredDir, getProps().TurnSpeed);
  moveForward(getCard().speed);
}

export function onResetGame() {
  delete getCard().speed;
}
