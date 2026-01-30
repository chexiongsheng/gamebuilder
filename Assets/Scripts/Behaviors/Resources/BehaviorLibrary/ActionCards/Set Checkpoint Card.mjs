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

import { getCardTargetActor } from "../../apiv2/actors/actors.mjs";
import { cooldown, send } from "../../apiv2/actors/messages.mjs";
import { propCardTargetActor } from "../../apiv2/actors/properties.mjs";
import { getPos } from "../../apiv2/transform/position-get.mjs";

export const PROPS = [
  propCardTargetActor("Target", {
    label: "Whose checkpoint?"
  }),
]

/**
 * @param {GActionMessage} actionMessage
 */
export function onAction(actionMessage) {
  const target = getCardTargetActor("Target", actionMessage);
  if (target) {
    send(target, 'SetCheckpoint', { pos: getPos(target) });
  }
  cooldown(1.0);
}
