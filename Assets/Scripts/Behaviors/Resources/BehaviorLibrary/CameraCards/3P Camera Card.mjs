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
import { clamp, degToRad, vec3, vec3add, vec3sub } from "../../apiv2/misc/math.mjs";
import { castAdvanced, CastMode } from "../../apiv2/physics/casting.mjs";
import { getLookAxes } from "../../apiv2/player_controls/controls.mjs";
import { getDistanceBetween, getPos, selfToWorldPos } from "../../apiv2/transform/position-get.mjs";
import { setPos } from "../../apiv2/transform/position-set.mjs";
import { getBackward, getForward, getRight, getYaw } from "../../apiv2/transform/rotation-get.mjs";
import { setYawPitchRoll } from "../../apiv2/transform/rotation-set.mjs";

export const PROPS = [
  propDecimal("HeadOffsetX", 0),
  propDecimal("HeadOffsetY", 2),
  propDecimal("HeadOffsetZ", 0),
  propDecimal("CamOffsetHoriz", 1),
  propDecimal("CamDistance", 5),
  propBoolean("IsPitchLocked", false),
  propDecimal("LockedPitch", -20),
  propDecimal("FieldOfView", 60),
]

const SPHERE_CAST_RADIUS_NORMAL = 0.3;
const SPHERE_CAST_RADIUS_LOOKING_UP = 0.55;

export function onCameraTick(msg) {
  if (!exists(msg.target)) return;
  const target = msg.target;
  reparentIfNeeded(target);

  // If yaw is unset (null), initialize with the target's yaw.
  if (getCard().yaw === null) {
    getCard().yaw = getYaw(target) || 0;
  }

  // Change yaw/pitch in response to user input.
  getCard().yaw += getLookAxes(target).x;
  getCard().pitch = clamp(getProps().IsPitchLocked ? degToRad(getProps().LockedPitch) : getCard().pitch + getLookAxes(target).y,
    degToRad(-80), degToRad(80));
  setYawPitchRoll(getCard().yaw, -getCard().pitch, 0);

  setPos(computeCameraPos(target));
  setCameraSettings({
    cursorActive: false,
    aimOrigin: getPos(),
    aimDir: getForward(),
    fov: getProps().FieldOfView || 60,
  });
}

function computeCameraPos(target) {
  const headPos = selfToWorldPos(vec3(getProps().HeadOffsetX, getProps().HeadOffsetY, getProps().HeadOffsetZ), target);

  // Figure out what radius to use for the sphere cast. When the player
  // is looking up, we use a bigger radius to prevent the camera from
  // clipping the ground.
  const sphereCastRadius = getForward().y > 0 ? SPHERE_CAST_RADIUS_LOOKING_UP :
    SPHERE_CAST_RADIUS_NORMAL;

  // Find the ideal camera position.
  const idealCamPos = vec3add(vec3add(headPos, getRight(getProps().CamOffsetHoriz)),
    getBackward(getProps().CamDistance));

  // If from that position I can see the player's head directly
  // with no terrain in the way, that's a good position for the camera.
  let hit = castAdvanced(
    idealCamPos,
    vec3sub(headPos, idealCamPos),
    getDistanceBetween(idealCamPos, headPos),
    sphereCastRadius, CastMode.CLOSEST,
    /* includeActors */ false,
    /* includeSelf */ false,
    /* includeTerrain */ true);
  if (!hit || hit.actor === target) {
    return idealCamPos;
  }

  // If we got here, it means a piece of terrain is in the way between
  // the player's head and the ideal camera position, so we have to do
  // things the hard way: first raycast to the side until we hit something,
  // then raycast back to see how far we can go with the camera in each
  // direction.
  let pos = headPos;
  if (Math.abs(getProps().CamOffsetHoriz) > 0.01) {
    const sign = getProps().CamOffsetHoriz > 0 ? 1 : -1;
    hit = castAdvanced(
      headPos,
      getRight(sign),
      Math.abs(getProps().CamOffsetHoriz),
      sphereCastRadius, CastMode.CLOSEST,
      /* includeActors */ false,
      /* includeSelf */ false,
      /* includeTerrain */ true);
    const allowedDist = hit ? hit.distance - 0.1 : getProps().CamOffsetHoriz;
    pos = vec3add(pos, getRight(sign * allowedDist));
  }
  hit = castAdvanced(
    pos,
    getBackward(),
    getProps().CamDistance,
    sphereCastRadius,
    CastMode.CLOSEST,
    /* includeActors */ false,
    /* includeSelf */ false,
    /* includeTerrain */ true);
  const allowedDist = hit ? hit.distance - 0.1 : getProps().CamDistance;
  pos = vec3add(pos, getBackward(allowedDist));
  return pos;
}

export function onInit() {
  getCard().yaw = null;  // null means "unset".
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