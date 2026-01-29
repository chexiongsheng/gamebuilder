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

import { getCard } from "../../apiv2/actors/memory.mjs";
import { propBoolean, propDecimal, propEnum } from "../../apiv2/actors/properties.mjs";
import { degToRad, min, vec3x, vec3y, vec3z } from "../../apiv2/misc/math.mjs";
import { deltaTime } from "../../apiv2/misc/time.mjs";
import { turn } from "../../apiv2/transform/rotation-set.mjs";

export const PROPS = [
  propDecimal("Degrees", 90),
  propBoolean("Counterclockwise", false),
  propDecimal("Speed", 90),
  propEnum("Axis", "Y", ["X", "Y", "Z"]),
];

export function onResetGame() {
  delete getCard().degreesLeft;
}

export function onAction() {
  getCard().degreesLeft = getProps().Degrees;
}

export function onTick() {
  if (getCard().degreesLeft) {
    const degreesToTurn = min(getCard().degreesLeft, getProps().Speed * deltaTime());
    getCard().degreesLeft -= degreesToTurn;
    turn((getProps().Counterclockwise ? -1 : 1) * degToRad(degreesToTurn),
      getProps().Axis === "X" ? vec3x() : getProps().Axis === "Z" ? vec3z() : vec3y());
    if (getCard().degreesLeft < 0.01) {
      delete getCard().degreesLeft;
    }
  }
}

export function getCardStatus() {
  return {
    description: `Turn <color=yellow>${getProps().Degrees} deg ${getProps().Counterclockwise ? 'counter' : ''}clockwise</color> ` +
      `at speed <color=green>${getProps().Speed.toFixed(1)}</color> about the <color=orange>${getProps().Axis} axis</color>`
  }
}
