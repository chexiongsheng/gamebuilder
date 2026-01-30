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

import { exists, getCardTargetActor, getCardTargetActorDescription } from "../../apiv2/actors/actors.mjs";
import { getDisplayName } from "../../apiv2/actors/attributes.mjs";
import { getCard } from "../../apiv2/actors/memory.mjs";
import { send, sendToSelfDelayed } from "../../apiv2/actors/messages.mjs";
import { getProps, propActor, propBoolean, propCardTargetActor, propDecimal, requireTrue } from "../../apiv2/actors/properties.mjs";
import { logError } from "../../apiv2/misc/utility.mjs";
import { getCameraActor, isPlayerControllable } from "../../apiv2/player_controls/controls.mjs";

export const PROPS = [
  propCardTargetActor("Target", {
    label: "What player?"
  }),
  propActor("NewCamera", "", {
    pickerPrompt: "Change to which camera?",
  }),
  propBoolean("Temporary", false),
  propDecimal("Duration", 5, {
    requires: [requireTrue("Temporary")]
  })
]

export function onInit() {
  delete getCard().waitingToRevert;
}

/**
 * @param {GActionMessage} actionMessage
 */
export function onAction(actionMessage) {
  const errMessage = getCardErrorMessage();
  if (errMessage) {
    logError(errMessage);
    return;
  }

  if (getCard().waitingToRevert) return;

  const playerActor = getCardTargetActor("Target", actionMessage);
  const oldCamera = changeCamera(playerActor, getProps().NewCamera);

  if (getProps().Temporary) {
    getCard().waitingToRevert = true;
    sendToSelfDelayed(getProps().Duration, "RevertCamera", {
      playerActor: playerActor,
      oldCamera: oldCamera
    });
  }
}

export function onRevertCamera(msg) {
  delete getCard().waitingToRevert;
  changeCamera(msg.playerActor, msg.oldCamera);
}

function changeCamera(playerActor, newCamera) {
  if (!exists(playerActor)) {
    logError("Change Camera: target player actor does not exist.");
    return;
  } else if (!isPlayerControllable(playerActor)) {
    logError("Change Camera: not a player controllable actor: " + getDisplayName(playerActor));
    return;
  }

  // Tell the old camera to target null, and the new camera to target the player.
  const oldCamera = getCameraActor(playerActor);
  if (exists(oldCamera)) {
    send(oldCamera, "AssignTarget", { target: null });
  }
  if (exists(newCamera)) {
    send(newCamera, "AssignTarget", { target: playerActor });
  }
  return oldCamera;
}

export function getCardErrorMessage() {
  if (!exists(getProps().NewCamera)) {
    return "* Error: NewCamera field must be set";
  }
  if (getProps().Target === 'SELF' && !isPlayerControllable()) {
    return "* Error: this actor is not a player";
  }
}

export function getCardStatus() {
  const dur = getProps().Temporary ? ` for <color=orange>${getProps().Duration.toFixed(1)}s</color>` : '';
  return {
    description: `Change camera of <color=yellow>${getCardTargetActorDescription('Target')}</color> to <color=green>${getDisplayName(getProps().NewCamera)}</color>${dur}`
  }
}