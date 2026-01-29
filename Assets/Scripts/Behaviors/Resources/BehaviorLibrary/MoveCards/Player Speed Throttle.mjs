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

export const PROPS = [
  propDecimal("Accel", 14, { label: "Accelerate speed" }),
  propDecimal("MaxSpeed", 16, { label: "Top speed" }),
  propDecimal("Slowdown", 5, { label: "Slow down (friction)" }),
  propDecimal("StopTime", .2, { label: "Time stopped (before reverse)" })
];

export function onActiveTick() {
  if (getAttrib("isDead", false) || getAttrib("ControlsLocked", false)) {
    moveGlobal(vec3zero());
    return;
  }

  getCard().speed = getCard().speed || 0;
  getCard().transitionTimer = getCard().transitionTimer || 0;

  const throttle = getThrottle().z;

  let decel = 0;
  if (getCard().speed > 0) decel = -props.Slowdown;
  else if (getCard().speed < 0) decel = props.Slowdown;

  const totalAccel = (throttle * props.Accel + decel) * deltaTime();

  if (getCard().goingForward) {
    getCard().speed = Math.max(0, Math.min(props.MaxSpeed, getCard().speed + totalAccel));
  } else {
    getCard().speed = Math.min(0, Math.max(-props.MaxSpeed, getCard().speed + totalAccel));
  }

  if (getCard().speed == 0 && throttle != 0) changeDirectionCheck();

  if (Math.abs(getCard().speed) < 0.1) {
    moveGlobal(vec3zero());
  } else {
    moveGlobal(getForward(getCard().speed));
  }

}

function changeDirectionCheck() {
  const throttle = getThrottle().z;

  const wantTransition =
    (throttle > 0 && !getCard().goingForward) ||
    (throttle < 0 && getCard().goingForward);

  if (wantTransition) {
    getCard().transitionTimer += deltaTime();
    if (getCard().transitionTimer >= props.StopTime) {
      getCard().transitionTimer = 0;
      getCard().goingForward = !getCard().goingForward;
    }
  } else {
    getCard().transitionTimer = 0;
  }
}

export function onResetGame() {
  delete getCard().speed;
}

