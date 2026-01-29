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

import { exists } from "../../apiv2/actors/actors.mjs";
import { setCameraSettings } from "../../apiv2/actors/camera_light.mjs";
import { getCard } from "../../apiv2/actors/memory.mjs";
import { getProps, propBoolean, propDecimal } from "../../apiv2/actors/properties.mjs";
import { attachToParent, detachFromParent, getParent } from "../../apiv2/hierarchy/parenting.mjs";
import { degToRad, vec3 } from "../../apiv2/misc/math.mjs";
import { getLookAxes } from "../../apiv2/player_controls/controls.mjs";
import { getPos, selfToWorldPos } from "../../apiv2/transform/position-get.mjs";
import { setPos } from "../../apiv2/transform/position-set.mjs";
import { getForward, getYaw } from "../../apiv2/transform/rotation-get.mjs";
import { setYawPitchRoll } from "../../apiv2/transform/rotation-set.mjs";

export const PROPS = [
  propDecimal("OffsetX", 0),
  propDecimal("OffsetY", 1.5),
  propDecimal("OffsetZ", 0),
  propBoolean("HidePlayer", true),
  propDecimal("FieldOfView", 60),
]

export function onCameraTick(msg) {
  if (!exists(msg.target)) return;
  const target = msg.target;
  reparentIfNeeded(target);

  // If yaw is unset (null), initialize with the target's yaw.
  if (getCard().yaw === null) {
    getCard().yaw = getYaw(target) || 0;
  }

  setPos(selfToWorldPos(vec3(getProps().OffsetX, getProps().OffsetY, getProps().OffsetZ), msg.target));
  getCard().yaw += getLookAxes(target).x;
  getCard().pitch = Math.min(Math.max(getCard().pitch + getLookAxes(target).y, degToRad(-80)), degToRad(80));
  setYawPitchRoll(getCard().yaw, -getCard().pitch, 0);
  setCameraSettings({
    cursorActive: false,
    aimOrigin: getPos(),
    aimDir: getForward(),
    dontRenderActors: getProps().HidePlayer ? [msg.target] : null,
    fov: getProps().FieldOfView || 60,
  });
}

export function onInit() {
  getCard().yaw = null;  // null means "unset"
  getCard().pitch = 0;
}

export function reparentIfNeeded(target) {
  if ((target || null) === (getParent() || null)) return;
  if (exists(target)) {
    attachToParent(target);
  } else {
    detachFromParent();
  }
}