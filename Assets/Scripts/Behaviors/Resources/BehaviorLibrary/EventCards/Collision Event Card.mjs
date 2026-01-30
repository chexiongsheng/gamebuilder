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

import { getActorGroupDescription, isActorInGroup } from "../../apiv2/actors/actors.mjs";
import { getCard } from "../../apiv2/actors/memory.mjs";
import { getProps, propActorGroup, propBoolean } from "../../apiv2/actors/properties.mjs";
import { getTime } from "../../apiv2/misc/time.mjs";
import { isVisible } from "../../apiv2/rendering/visibility.mjs";

export const PROPS = [
  propActorGroup("withWhat", "@ANY", {
    label: "Collide with what:",
    pickerPrompt: "When I collide with what?",
  }),
  propBoolean("ignoreHidden", false, {
    label: "Ignore hidden actors"
  })
];

// Collision is a dirty signal, so once we trigger we hold the signal for
// this amount of time to clean it up.
export const COLLISION_STICKY_DURATION = 0.2;

export function onCollision(msg) {
  // (hack: some legacy files don't have this set)
  getProps().withWhat = getProps().withWhat === undefined ? "@ANY" : getProps().withWhat;

  if (!isActorInGroup(msg.other, getProps().withWhat)) {
    return;
  }

  if (getProps().ignoreHidden && !isVisible(msg.other)) {
    return;
  }

  /** @type {GEvent} */
  getCard().triggeredEvent = {
    actor: msg.other
  };
  getCard().stickyUntil = getTime() + COLLISION_STICKY_DURATION;
}

// onCheck isn't necessarily always called, so it's important we clear our
// triggered event on reset.
export function onResetGame() {
  delete getCard().triggeredEvent;
  delete getCard().stickyUntil;
}

/**
 * @return {GEvent|undefined} The event, if one occurred.
 */
export function onCheck() {
  if (getCard().triggeredEvent !== undefined) {
    const rv = getCard().triggeredEvent;
    if (getTime() > getCard().stickyUntil) {
      delete getCard().stickyUntil;
      delete getCard().triggeredEvent;
    }
    return rv;
  }
  else {
    return undefined;
  }
}

export function getCardStatus() {
  const groupName = getActorGroupDescription(getProps().withWhat, true);
  return {
    description: "When I collide with <color=yellow>" + groupName
  }
}
