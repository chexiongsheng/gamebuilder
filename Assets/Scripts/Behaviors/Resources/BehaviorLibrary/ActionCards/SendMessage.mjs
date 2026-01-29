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

import { getActorGroupDescription, getActorsInGroup } from "../../apiv2/actors/actors.mjs";
import { sendToMany, sendToManyDelayed } from "../../apiv2/actors/messages.mjs";
import { getProps, propActorGroup, propDecimal, propString } from "../../apiv2/actors/properties.mjs";

export const PROPS = [
  propActorGroup("Recipient", "", {
    label: "Recipient",
    pickerPrompt: "Who should I send to?",
    allowOffstageActors: true
  }),
  propString("Message", "Ping"),
  propDecimal("Delay", 0, {
    label: "Delay (sec)"
  })
];

/**
 * @param {GActionMessage} actionMessage 
 */
export function onAction(actionMessage) {
  const targets = getActorsInGroup(getProps().Recipient);
  if (getProps().Delay > 0) {
    sendToManyDelayed(getProps().Delay, targets, getProps().Message);
  } else {
    sendToMany(targets, getProps().Message);
  }
}

export function getCardErrorMessage() {
  if (!getProps().Recipient) {
    return "NEED RECIPIENT: Click card to fix.";
  }
}

export function getCardStatus() {
  let delay = '';
  if (getProps().Delay > 0) {
    delay = ` with delay <color=orange>${getProps().Delay.toFixed(1)}</color>`;
  }
  return {
    description: `Send message <color=yellow>${getProps().Message}</color> to <color=green>${getActorGroupDescription(getProps().Recipient)}</color>${delay}`
  }
}