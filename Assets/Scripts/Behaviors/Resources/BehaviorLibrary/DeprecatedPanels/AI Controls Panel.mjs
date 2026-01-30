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

import { getPlayerActors } from "../../apiv2/actors/actors.mjs";
import { callActionDeck } from "../../apiv2/actors/messages.mjs";
import { propDeck, propDecimal } from "../../apiv2/actors/properties.mjs";
import { radToDeg } from "../../apiv2/misc/math.mjs";
import { getPos } from "../../apiv2/transform/position-get.mjs";
import { getForward } from "../../apiv2/transform/rotation-get.mjs";

export const PROPS = [
  propDeck('idleDeck', 'Action', {
    label: "What do I do when I don't see the player?",
    deckOptions: {
      defaultCardURIs: ['builtin:MoveRandomWalk']
    }
  }),
  propDecimal("VisibleRange", 10),
  propDeck('seePlayerDeck', 'Action', {
    label: 'What do I do if I see the player?',
    deckOptions: {
      defaultCardURIs: ['builtin:MoveRandomWalk']
    }
  }),
]

export function onTick() {
  var isSeeing = false;
  for (let actor of getPlayerActors()) {
    const toTarget = getPos(actor).sub(getPos());
    const dist = toTarget.length();

    if (dist > getProps().VisibleRange) {
      continue;
    }

    toTarget.normalize();
    const forward = getForward();
    const degreesOff = radToDeg(forward.angleTo(toTarget));

    if (degreesOff > 15) {
      continue;
    }

    // TODO ray cast?
    callActionDeck("seePlayerDeck", { event: { actor: actor } });
    isSeeing = true;
    break;
  }
  if (!isSeeing) {
    callActionDeck("idleDeck");
  }
}
