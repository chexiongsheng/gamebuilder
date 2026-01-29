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
import { getMem } from "../../apiv2/actors/memory.mjs";
import { propDecimal } from "../../apiv2/actors/properties.mjs";
import { legacyApi } from "../../apiv2/apiv2.mjs";
import { vec3 } from "../../apiv2/misc/math.mjs";
import { deltaTime } from "../../apiv2/misc/time.mjs";
import { addVelocity } from "../../apiv2/physics/velocity.mjs";
import { enableGravity, getAimDirection, getWorldThrottle, isGrounded, isSprinting, lookDir, moveGlobal } from "../../apiv2/player_controls/controls.mjs";

// Player Controls: Basic WASD.

export const PROPS = [
  propDecimal("Speed", 8),
  propDecimal("SprintSpeed", 12),
  propDecimal("JumpHeight", 1.8)
];

export function onControl() {
  enableGravity(true);
  if (getAttrib("isDead", false)) {
    return;
  }
  getMem().jumpCooldown = Math.max(0, (getMem().jumpCooldown || 0) - deltaTime());
  const velocity = getWorldThrottle();
  velocity.multiplyScalar(isSprinting() ? getProps().SprintSpeed : getProps().Speed);
  moveGlobal(velocity);
  lookDir(getAimDirection(), true);
}

export function onJump() {
  if (getAttrib("isDead", false)) {
    return;
  }
  if (isGrounded() && getMem().jumpCooldown <= 0) {
    getMem().jumpCooldown = 0.1;
    // The 50 is additional gravity we put on the player character to prevent
    // floatiness.
    const gravity = 9.81 + 50;
    const jumpSpeed = Math.sqrt((getProps().JumpHeight || 1.8) * 2 * gravity);
    addVelocity(vec3(0, jumpSpeed, 0));
    legacyApi().sendMessageToUnity("Jumped");

  }
}