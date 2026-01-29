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
  propString("Message", "Hi, how are you?"),
  propDecimal("OffsetAbove", 0),
  propNumber("TextSize", 1),
  propDecimal("HideDelay", 3)
];

const INITIAL_OFFSET_ABOVE = 1;
const INITIAL_SCALE = 6;

/**
 * @param {GActionMessage} actionMessage 
 */
export function onAction(actionMessage) {
  // Create popup text if we don't have it yet.
  if (!getCard().popupTextActor || !exists(getCard().popupTextActor)) {
    getCard().popupTextActor = clone("builtin:PopupText",
      getPointAbove(getBoundsSize().y + INITIAL_OFFSET_ABOVE + getProps().OffsetAbove));
    const scale = Math.min(Math.max(INITIAL_SCALE + (getProps().TextSize || 0) * 0.5, 1), 12);
    send(getCard().popupTextActor, "SetText", { text: getProps().Message });
    send(getCard().popupTextActor, "SetScale", scale);
    send(getCard().popupTextActor, "SetParent", { parent: myself() });
  }
  getCard().popupHideTime = getTime() + (getProps().HideDelay === undefined ? HIDE_DELAY_SECONDS : getProps().HideDelay);
}

export function onResetGame() {
  // Popup is a script clone, so it gets destroyed automatically.
  delete getCard().popupTextActor;
  delete getCard().popupHideTime;
}

export function onTick() {
  if (getCard().popupHideTime && getTime() > getCard().popupHideTime) {
    // Hide the popup.
    send(getCard().popupTextActor, "Destroy");
    delete getCard().popupTextActor;
    delete getCard().popupHideTime;
  }
}

export function getCardStatus() {
  let msg = getProps().Message;
  if (msg.length > 20) {
    msg = msg.substr(0, 20) + '[...]'
  }
  return {
    description: `Says '<color=yellow>${msg}</color>'`
  }
}