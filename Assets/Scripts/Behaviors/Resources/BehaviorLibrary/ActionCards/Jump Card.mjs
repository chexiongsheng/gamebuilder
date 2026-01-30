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
import { getCard, getMem } from "../../apiv2/actors/memory.mjs";
import { getProps, propDecimal } from "../../apiv2/actors/properties.mjs";
import { legacyApi } from "../../apiv2/apiv2.mjs";
import { vec3 } from "../../apiv2/misc/math.mjs";
import { deltaTime, getTime } from "../../apiv2/misc/time.mjs";
import { addVelocity } from "../../apiv2/physics/velocity.mjs";
import { isGrounded, isPlayerControllable } from "../../apiv2/player_controls/controls.mjs";

// Player Controls: Basic WASD.

export const PROPS = [
  propDecimal("JumpHeight", 1.8)
];

export function onTick() {
  if (getAttrib("isDead", false)) {
    return;
  }
  getMem().jumpCooldown = Math.max(0, (getMem().jumpCooldown || 0) - deltaTime());

  if (isGrounded()) {
    getCard().lastGroundedTime = getTime();
  }
}

export function onAction() {
  if (getAttrib("isDead", false)) {
    return;
  }

  const timeSinceGround = getTime() - (getCard().lastGroundedTime || 0);
  // Allow some coyote-time.
  const sortOfGrounded = isGrounded() || timeSinceGround < 0.1;

  // If we're a player and not grounded, don't jump.
  if (isPlayerControllable() && !sortOfGrounded) return;

  if (getMem().jumpCooldown <= 0) {
    getMem().jumpCooldown = 0.1;
    // After jumping we don't want to allow an extra coyote-jump while in air.
    delete getCard().lastGroundedTime;
    // The 50 is additional gravity we put on the player character to prevent
    // floatiness.
    const gravity = 9.81 + 50;
    const jumpSpeed = Math.sqrt((getProps().JumpHeight || 1.8) * 2 * gravity);
    addVelocity(vec3(0, jumpSpeed, 0));
    legacyApi().sendMessageToUnity("Jumped");
  }
}

export function getCardStatus() {
  return {
    description: `Jump with height <color=green>${getProps().JumpHeight.toFixed(1)}</color>`
  }
}