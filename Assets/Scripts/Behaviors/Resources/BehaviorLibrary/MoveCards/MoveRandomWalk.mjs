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

// Random walk

export const PROPS = [
  propDecimal("Speed", 1),
  propDecimal("MinTurn", 10),
  propDecimal("MaxTurn", 180),
  propDecimal("MinDistBetweenTurns", 1),
  propDecimal("MaxDistBetweenTurns", 5),
  propDecimal("TurnSpeed", 90), //degrees per second
  propBoolean("MoveWhileTurning", true),
  propBoolean("LimitRange", false),
  propDecimal("MaxRange", 10, { requires: [requireTrue("LimitRange")] }),
]

//a better way to do this?
export function onResetGame() {
  getCard().walkDist = 0;
  getCard().walkDistTarget = 0;
  getCard().turnAmount = 0
  getCard().turnAmountTarget = 0;
  getCard().returningHome = false;
  newTargets();
}

export function onActiveTick() {
  if (getCard().returningHome) {
    returnHomeUpdate();
    return;
  }
  // checks if it reach dist then turn, if neither, just move a bit
  if (reachedTargetDist()) {
    if (reachedTargetTurn()) {
      newTargets();
    } else {
      turnUpdate();
    }
  } else {
    getCard().walkDist += getProps().Speed * deltaTime();
    moveForward(getProps().Speed);
  }

  if (getProps().LimitRange && getProps().MaxRange < getDistanceTo(getSpawnPos())) {
    // Wandered too far! Time to go home.
    getCard().returningHome = true;
  }
}

function turnUpdate() {
  //if speed is 0, just snap to target turn, get new targets, and return
  if (getProps().TurnSpeed == 0) {
    turn(getCard().turnAmountTarget);
    newTargets();
    return;
  }

  const turnDelta = degToRad(getProps().TurnSpeed * deltaTime())
    * Math.sign(getCard().turnAmountTarget - getCard().turnAmount);
  getCard().turnAmount += turnDelta
  turn(turnDelta);

  if (getProps().MoveWhileTurning) {
    moveForward(getProps().Speed);
  }
}

// grabs a new distance to walk and turn
function newTargets() {
  getCard().walkDist = 0;
  getCard().walkDistTarget = THREE.Math.randFloat(
    getProps().MinDistBetweenTurns, getProps().MaxDistBetweenTurns)

  getCard().turnAmount = 0;
  let randomDegrees = THREE.Math.randFloat(
    getProps().MinTurn, getProps().MaxTurn);

  //make it randomly be clockwise versus counterclockwise
  randomDegrees = randomDegrees * (Math.random() > .5 ? -1 : 1);

  getCard().turnAmountTarget = degToRad(randomDegrees);
}

// have we reached that distance?
function reachedTargetDist() {
  return (getCard().walkDist || 0) >= (getCard().walkDistTarget || 0);
}

// have we reached the target turn amount?
function reachedTargetTurn() {
  return Math.abs((getCard().turnAmountTarget || 0) - (getCard().turnAmount || 0)) < .01;
}

function returnHomeUpdate() {
  const forward = getForward();
  const toHome = vec3sub(getSpawnPos(), getPos());
  const angleToHomeDegrees = radToDeg(forward.angleTo(toHome));
  lookToward(getSpawnPos(), getProps().TurnSpeed > 0 ? degToRad(getProps().TurnSpeed) : 20, true);
  if (vec3dot(getForward(), toHome) > 0 && angleToHomeDegrees < 30) {
    moveForward(getProps().Speed);
  }
  if (getDistanceTo(getSpawnPos()) < getProps().MaxRange / 2) {
    getCard().returningHome = false;
  }
}

export function getCardStatus() {
  return {
    description: `Wanders around randomly with speed <color=green>${getProps().Speed.toFixed(1)}</color>`
  }
}
