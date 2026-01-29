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

import { exists, getCardTargetActor, getCardTargetActorDescription, myself } from "../../apiv2/actors/actors.mjs";
import { getDisplayName } from "../../apiv2/actors/attributes.mjs";
import { getProps, propActor, propCardTargetActor } from "../../apiv2/actors/properties.mjs";
import { setPosPlease } from "../../apiv2/remote/remote.mjs";
import { getPos } from "../../apiv2/transform/position-get.mjs";
import { setPos } from "../../apiv2/transform/position-set.mjs";

export const PROPS = [
  propCardTargetActor('Teleportee', {
    label: 'Teleport what'
  }),
  propActor('DestinationActor', '', {
    label: 'Teleport to:',
    pickerPrompt: 'Teleport to?'
  }),
]

export function getCardErrorMessage() {
  if (!exists(getProps().DestinationActor)) {
    return "*** Need a destination actor.";
  }
}

/** @type {GActionMessage} actionMessage */
export function onAction(actionMessage) {
  const teleportee = getCardTargetActor('Teleportee', actionMessage);
  if (teleportee === myself()) {
    setPos(getPos(getProps().DestinationActor));
  } else if (exists(teleportee)) {
    setPosPlease(teleportee, getPos(getProps().DestinationActor));
  }
}

export function getCardStatus() {
  return {
    description: `Teleport <color=yellow>${getCardTargetActorDescription('Teleportee')}</color> to where <color=green>${getDisplayName(getProps().DestinationActor)}</color> is.`
  }
}