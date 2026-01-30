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
import { getProps, propNumber } from "../../apiv2/actors/properties.mjs";
import { ApiV2Context } from "../../apiv2/apiv2.mjs";
import { getTime } from "../../apiv2/misc/time.mjs";
import { log } from "../../apiv2/misc/utility.mjs";;

export const PROPS = [
  propNumber("Delay", 3)
]

/** @param {GActionMessage} actionMessage */
export function onAction(actionMessage) {
  getCard().resetTime = getTime() + getProps().Delay;
}

export function onTick() {
  if (getCard().resetTime && getTime() > getCard().resetTime) {
    log("Resetting...");
    delete getCard().resetTime;
    ApiV2Context.instance.api.sendMessageToAll("ResetGame");
  }
}

export function onResetGame() {
  delete getCard().resetTime;
}
