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

import { getVar } from "../../apiv2/actors/attributes.mjs";
import { getLocalCameraPos } from "../../apiv2/actors/camera_light.mjs";
import { getMem } from "../../apiv2/actors/memory.mjs";
import { propBoolean, propColor, propDecimal, propString } from "../../apiv2/actors/properties.mjs";
import { clamp, vec3normalized, vec3sub } from "../../apiv2/misc/math.mjs";
import { castAdvanced, CastMode } from "../../apiv2/physics/casting.mjs";
import { getBoundsCenter } from "../../apiv2/rendering/body.mjs";
import { getDistanceBetween } from "../../apiv2/transform/position-get.mjs";
import { getScreenSphere } from "../../apiv2/ui/screen.mjs";
import { uiRect } from "../../apiv2/ui/widgets.mjs";;
import { raycast } from "../../apiv2/physics/casting.mjs";
import { getProps } from "../../apiv2/actors/properties.mjs";

export const PROPS = [
  propDecimal('OffsetY', 1),
  propDecimal('Size', 1),
  propColor('FgHigh', '#00ff00'),
  propColor('BgHigh', '#002000'),
  propColor('FgMedium', '#ffff00'),
  propColor('BgMedium', '#202000'),
  propColor('FgLow', '#ff0000'),
  propColor('BgLow', '#200000'),
  propString('AttribCur', "health"),
  propString('AttribMax', "startingHealth"),
  propDecimal('Opacity', 0.8),
  propBoolean('OnlyIfVisible', true, {
    label: "Only draw if visible"
  }),
]

export function onDrawScreen() {
  const worldAnchor = getBoundsCenter();
  worldAnchor.y += getProps().OffsetY;

  if (getProps().OnlyIfVisible) {
    // Only draw if the world anchor position is visible from the camera.
    const cameraPos = getLocalCameraPos();
    if (cameraPos === null) return;
    const cameraToAnchor = vec3normalized(vec3sub(worldAnchor, cameraPos));
    const maxDist = Math.max(0, getDistanceBetween(cameraPos, worldAnchor) - 0.5);
    // Request boolean raycast with actors and terrain, exclude self:
    if (castAdvanced(cameraPos, cameraToAnchor, maxDist, 0, CastMode.BOOLEAN, true, false, true)) {
      // Hit some obstruction.
      return;
    }
  }

  const screenSphere = getScreenSphere(worldAnchor, getProps().Size);

  if (!screenSphere) return;  // Off-screen.

  const width = screenSphere.radius;
  const height = width * 0.2;
  const left = screenSphere.center.x - width / 2;
  const top = screenSphere.center.y - height;

  const cur = getVar("isDead") ? 0 : (getVar(getProps().AttribCur) || getMem()[getProps().AttribCur] || 0);
  const max = Math.max(1, getVar(getProps().AttribMax) || getMem()[getProps().AttribMax] || 0);
  const fraction = clamp(cur / max, 0, 1);

  const fgColor = getColorPropForHealthFraction(fraction, 'FgLow', 'FgMedium', 'FgHigh');
  const bgColor = getColorPropForHealthFraction(fraction, 'BgLow', 'BgMedium', 'BgHigh');

  // Background.
  uiRect(left, top, width, height, bgColor, { opacity: getProps().Opacity });
  // Filled part.
  uiRect(left, top, width * fraction, height, fgColor, { opacity: getProps().Opacity });
  // Border.
  uiRect(left, top, width, height, fgColor, { style: "BORDER", opacity: getProps().Opacity });
}

function getColorPropForHealthFraction(fraction, propLow, propMedium, propHigh) {
  return fraction > 0.75 ? getProps()[propHigh] :
    fraction > 0.25 ? getProps()[propMedium] : getProps()[propLow];
}