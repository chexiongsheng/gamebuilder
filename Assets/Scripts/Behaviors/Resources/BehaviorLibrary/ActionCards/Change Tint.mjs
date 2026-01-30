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

import * as THREE from "three.mjs";
import { getCard } from "../../apiv2/actors/memory.mjs";
import { getTime } from "../../apiv2/misc/time.mjs";
import { getTintHex, setTintColor, setTintHex } from "../../apiv2/rendering/color.mjs";
import { propNumber } from "../../apiv2/actors/properties.mjs";

export const PROPS = [
  //TEMP propNumber("Duration", 1),
]

/** @param {GActionMessage} actionMessage */
export function onAction(actionMessage) {
  getCard().originalTintHex = getCard().originalTintHex || getTintHex();
  // setTintColor(new THREE.Color(
  //  getProps().ColorR / 255.0, getProps().ColorG / 255.0, getProps().ColorB / 255.0));
  setTintColor(new THREE.Color(1, 0, 0));
  // TEMP getCard().restoreTintTime = getTime() + getProps().Duration;
  getCard().restoreTintTime = getTime() + 0.5;
}

export function onTick() {
  if (getCard().restoreTintTime) {
    if (getTime() > getCard().restoreTintTime) {
      delete getCard().restoreTintTime;
      setTintHex(getCard().originalTintHex);
    }
    else {
      const red = Math.floor(getTime() * 6) % 2 == 0;
      setTintHex(red ? "#ff0000" : getCard().originalTintHex);
    }
  }
}