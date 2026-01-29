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

import { getMem } from "../../apiv2/actors/memory.mjs";
import { getProps, propColor } from "../../apiv2/actors/properties.mjs";
import { colorToHex } from "../../apiv2/misc/colors.mjs";
import { getTintHex, setTintColor, setTintHex } from "../../apiv2/rendering/color.mjs";

export const PROPS = [
  propColor("Color", "#0000ff")
];

/**
 * @param {GActionMessage} actionMessage 
 */
export function onAction(actionMessage) {
  // Store original tint in actor memory rather than card memory,
  // for correct restore behavior in case we have more than one
  // color-changing card.
  getMem().origTintHex = getMem().origTintHex || getTintHex();
  setTintColor(getProps().Color);
}

export function onResetGame() {
  if (getMem().origTintHex) {
    setTintHex(getMem().origTintHex);
    delete getMem().origTintHex;
  }
}

export function getCardStatus() {
  return {
    description: `Change color to: <color=${colorToHex(getProps().Color)}>${colorToHex(getProps().Color)}</color>`
  }
}