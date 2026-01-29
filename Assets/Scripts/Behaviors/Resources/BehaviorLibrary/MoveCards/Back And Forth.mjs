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
  propDecimal("DistForward", 5),
  propDecimal("DistUp", 0),
  propDecimal("DistRight", 0),
  propDecimal("Speed", 3),
  propDecimal("RestTime", 1)
];

export function onResetGame() {
  delete getCard().startPos;
  delete getCard().going;
  delete getCard().restUntil;
}

export function onActiveTick() {
  // If we didn't compute the motion parameters yet or if the user changed
  // the spawn position, recompute.
  if (!getCard().startPos || !vec3equal(getSpawnPos(), getCard().startPos, 0.01)) {
    resetMotion();
  }
  if (getTime() < getCard().restUntil) return;
  const targetPos = getCard().going ? getEndPos() : getCard().startPos;
  const distToTarget = getDistanceTo(targetPos);
  const speed = interp(0, 0.25, 1, 1, distToTarget) * getProps().Speed;
  moveToward(targetPos, speed);
  if (distToTarget < 0.1) {
    // Close enough to target position. Rest a bit, then invert motion.
    getCard().restUntil = getTime() + getProps().RestTime;
    getCard().going = !getCard().going;
  }
}

function getEndPos() {
  return vec3add(getSpawnPos(),
    selfToWorldDir(vec3(getProps().DistRight, getProps().DistUp, getProps().DistForward)));
}

function resetMotion() {
  getCard().startPos = getSpawnPos();
  getCard().going = true;
  getCard().restUntil = getTime() + getProps().RestTime;
}

export function getCardStatus() {
  getTemp().dirList = getTemp().dirList || [];
  getTemp().dirList.length = 0;
  getDirDescription(getProps().DistForward, "forward", "back", getTemp().dirList);
  getDirDescription(getProps().DistUp, "up", "down", getTemp().dirList);
  getDirDescription(getProps().DistRight, "right", "left", getTemp().dirList);
  return {
    description: `Moves back and forth (<color=yellow>${getTemp().dirList.join(', ')}</color>) with speed <color=yellow>${getProps().Speed.toFixed(1)}</color>, rest time <color=yellow>${getProps().RestTime.toFixed(1)}</color>.`
  }
}

function getDirDescription(dist, positiveWord, negativeWord, list) {
  if (dist > 0.01) {
    list.push(dist.toFixed(1) + " " + positiveWord);
  } else if (dist < -0.01) {
    list.push((-dist).toFixed(1) + " " + negativeWord);
  }
}
