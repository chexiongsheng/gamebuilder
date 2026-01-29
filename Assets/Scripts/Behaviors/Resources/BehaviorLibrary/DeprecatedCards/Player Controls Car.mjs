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

// Player Controls: Car.

export const PROPS = [
  propDecimal("Accel", 8),
  propDecimal("MaxSpeed", 16),
  propDecimal("BrakeAccel", 32),
];

export function onControl() {
  const COAST_ACC = 2;
  const TURN_SENSIVITY = 0.15;

  getCard().speed = getCard().speed || 0;

  enableGravity(true);
  enableKeepUpright(true);

  if (getAttrib("isDead", false)) {
    return;
  }
  const throttle = getThrottle();

  if (throttle.z > 0) {
    getCard().speed += deltaTime() * getProps().Accel * (getCard().reverse ? -1 : 1);
  } else {
    const brakeAccel = throttle.z < -0.1 ? getProps().BrakeAccel : COAST_ACC;
    getCard().speed = getCard().speed > 0 ?
      Math.max(0, getCard().speed - brakeAccel * deltaTime()) :
      Math.min(0, getCard().speed + brakeAccel * deltaTime());
  }
  getCard().speed = Math.min(Math.max(getCard().speed, -getProps().MaxSpeed), getProps().MaxSpeed);
  moveGlobal(getForward(getCard().speed));

  turn(throttle.x * TURN_SENSIVITY * deltaTime() * getCard().speed);

  if (getCard().reverse) {
    uiRect(750, 300, 130, 40, UiColor.BLACK);
    uiText(760, 310, "REVERSE", UiColor.RED);
  }

  uiText(1200, 800, "[X]: Toggle REVERSE", UiColor.WHITE);
}

export function onResetGame() {
  delete getCard().speed;
  delete getCard().reverse;
}

export function onKeyDown(msg) {
  if (msg.keyName === "x") {
    getCard().reverse = !getCard().reverse;
  }
}
