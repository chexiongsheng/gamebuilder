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
  propDecimal("Speed", 5)
]

/** @type {GActionMessage} actionMessage */
export function onAction(actionMessage) {
  if (getProps().Speed <= 0) return;
  getCard().targetPos = selfToWorldPos(vec3(getProps().DistRight, getProps().DistUp, getProps().DistForward));
  getCard().endTime = getTime() + 1 + getDistanceTo(getCard().targetPos) / getProps().Speed;
}

export function onTick() {
  if (!getCard().targetPos) return;
  // Move towards target point, at given speed.
  moveToward(getCard().targetPos, getProps().Speed);
  // Stop trying to move after enough time has elapsed.
  if (getTime() > getCard().endTime) {
    resetCard();
  }
}

function resetCard() {
  delete getCard().targetPos;
  delete getCard().endTime;
}

export function onResetGame() {
  resetCard();
}

export function getCardStatus() {
  getTemp().dirList = getTemp().dirList || [];
  getTemp().dirList.length = 0;
  getDirDescription(getProps().DistForward, "forward", "back", getTemp().dirList);
  getDirDescription(getProps().DistUp, "up", "down", getTemp().dirList);
  getDirDescription(getProps().DistRight, "right", "left", getTemp().dirList);
  return {
    description: `Moves (<color=green>${getTemp().dirList.join(', ')}</color>) with speed <color=yellow>${getProps().Speed.toFixed(1)}`
  }
}

function getDirDescription(dist, positiveWord, negativeWord, list) {
  if (dist > 0.01) {
    list.push(dist.toFixed(1) + " " + positiveWord);
  } else if (dist < -0.01) {
    list.push((-dist).toFixed(1) + " " + negativeWord);
  }
}
