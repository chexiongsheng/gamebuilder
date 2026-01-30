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
import { getCard } from "../../apiv2/actors/memory.mjs";
import { propActor, propNumber } from "../../apiv2/actors/properties.mjs";
import { clamp, vec3, vec3equal, vec3normalized, vec3scale } from "../../apiv2/misc/math.mjs";
import { deltaTime } from "../../apiv2/misc/time.mjs";
import { setKinematic } from "../../apiv2/physics/physics.mjs";
import { lookDir } from "../../apiv2/player_controls/controls.mjs";
import { getDistanceBetween, getPos } from "../../apiv2/transform/position-get.mjs";
import { setPos } from "../../apiv2/transform/position-set.mjs";
import { getForward } from "../../apiv2/transform/rotation-get.mjs";
import { assertNumber, assertVector3 } from "../../util.mjs";

export const PROPS = [
  propNumber('SmoothFactor', 10),
  propActor('InitialDestination', '', {
    pickerPrompt: 'Initial Destination?'
  }),
  propNumber('InitialSpeed', 5)
];

export function onInit() {
  delete getCard().spline;
  if (exists(getProps().InitialDestination)) {
    onSetDestination({
      pos: getPos(getProps().InitialDestination),
      forward: getForward(1, getProps().InitialDestination),
      speed: getProps().InitialSpeed
    });
  }
}

export function onSetDestination(msg) {
  assertVector3(msg.pos, 'AutoPilot: msg.pos');
  assertVector3(msg.forward, 'AutoPilot: msg.forward');
  assertNumber(msg.speed, 'AutoPilot: msg.number');
  recomputeSpline(msg.pos, msg.forward, msg.speed);
}

export function onActiveTick() {
  if (!getCard().spline) return;
  // Must be kinematic for this to work.
  setKinematic(true);

  const dsdp = Math.max(calcLocalDsDp(getCard().spline.xfunc, getCard().spline.yfunc, getCard().spline.zfunc, getCard().spline.p), 0.001);
  const dsdt = getCard().spline.speed;
  const dpdt = dsdt / dsdp;
  const newP = getCard().spline.p + deltaTime() * dpdt;
  getCard().spline.p = clamp(newP, 0, 1);
  const desiredPos = getDesiredPos();
  const desiredVel = vec3scale(getDesiredVelocityP(), dpdt);
  setPos(desiredPos);
  lookDir(vec3normalized(desiredVel));
  if (newP >= 1) {
    // Arrived.
    delete getCard().spline;
  }
}

function getDesiredPos() {
  return vec3(
    polynomialEval(getCard().spline.xfunc, getCard().spline.p),
    polynomialEval(getCard().spline.yfunc, getCard().spline.p),
    polynomialEval(getCard().spline.zfunc, getCard().spline.p)
  );
}

function getDesiredVelocityP() {
  return vec3(
    polynomialEval(getCard().spline.dxdp, getCard().spline.p),
    polynomialEval(getCard().spline.dydp, getCard().spline.p),
    polynomialEval(getCard().spline.dzdp, getCard().spline.p)
  );
}

// Figures out the spline equation given the current position/orientation and the
// goal position/orientation.
function recomputeSpline(goalPos, goalForward, speed) {
  // I always wanted to write a function called recomputeSpline because it's such
  // a cool name. I wish this function was as cool as the name implies.
  if (getCard().spline && vec3equal(getCard().spline.goalPos, goalPos) && vec3equal(getCard().spline.goalForward, goalForward)) {
    // Redundant.
    return;
  }
  const startPos = getPos();
  const startForward = getForward();
  const smoothFactor = clamp(getProps().SmoothFactor, 2, 200);
  getCard().spline = {
    goalPos: goalPos,
    goalForward: goalForward,
    speed: speed,
    xfunc: polynomialFit(startPos.x, goalPos.x, startForward.x * smoothFactor, goalForward.x * smoothFactor),
    yfunc: polynomialFit(startPos.y, goalPos.y, startForward.y * smoothFactor, goalForward.y * smoothFactor),
    zfunc: polynomialFit(startPos.z, goalPos.z, startForward.z * smoothFactor, goalForward.z * smoothFactor),
    p: 0
  };
  getCard().spline.dxdp = polynomialDerivative(getCard().spline.xfunc);
  getCard().spline.dydp = polynomialDerivative(getCard().spline.yfunc);
  getCard().spline.dzdp = polynomialDerivative(getCard().spline.zfunc);
}

// Finds coefficients for a quadratic OR cubic polynomial f(p) such that:
//   f(0) = a, f(1) = b, f'(0) = u, f'(1) = v
// Where f' = df/dp.
// The coefficients are alpha, beta, gamma, delta for the polynomial:
//   f(p) = alpha * p^3 + beta * p^2 + gamma * p + delta
function polynomialFit(a, b, u, v) {
  // Try a quadratic fit first because it's simpler (but not guaranteed to exist).
  // If not, use cubic (guaranteed to exist).
  return quadraticFit(a, b, u, v) || cubicFit(a, b, u, v);
}

// Finds coefficients for a cubic polynomial f(p) such that:
//   f(0) = a, f(1) = b, f'(0) = u, f'(1) = v
// Where f' = df/dp.
// The coefficients are alpha, beta, gamma, delta for the polynomial:
//   f(p) = alpha * p^3 + beta * p^2 + gamma * p + delta
function cubicFit(a, b, u, v) {
  return {
    alpha: 2 * a - 2 * b + u + v,
    beta: 3 * b - 3 * a - 2 * u - v,
    gamma: u,
    delta: a
  };
}

// Finds the coefficients for a quadratic polynomial f(p) such that
//   f(0) = a, f(1) = b, f'(0) = u, f'(1) = v
// Where f' = df/dp.
// The coefficients are alpha, beta, gamma, delta for the polynomial:
//   f(p) = beta * p^2 + gamma * p + delta
// THIS IS NOT GUARANTEED (there might not be such a polynomial).
// If there is no such polynomial, returns null.
function quadraticFit(a, b, u, v) {
  // Requirement for the polynomial to exist u + v = 2b - 2a.
  // We'll accept an approximation and "cheat" by tweaking v if it's
  // close enough.
  const deviation = 2 * b - 2 * a - u - v;
  if (Math.abs(deviation) > 2) return null;
  // Let's fix v to make the math work. You didn't see anything. Move along.
  v += deviation;
  return {
    alpha: 0,
    beta: v - b + a,
    gamma: u,
    delta: a
  }
}

// Computes the derivative of the given polynomial.
function polynomialDerivative(cf) {
  // d/dp (A p^3 + B p^2 + C p + D) = 3 A p^2 + 2 B p + C
  return {
    alpha: 0,
    beta: 3 * cf.alpha,
    gamma: 2 * cf.beta,
    delta: cf.gamma
  };
}

// Evaluates the given cubic polynomial at the given value of p.
function polynomialEval(cf, p) {
  return cf.alpha * p * p * p +
    cf.beta * p * p +
    cf.gamma * p +
    cf.delta;
}

// Given functions for x, y and z, returns the approximate derivate ds/dp at p,
// where s is the distance along the curve. That is, this returns at what rate
// the curve is being traced with respect to p.
function calcLocalDsDp(xf, yf, zf, p) {
  const DELTA_P = 0.01;
  const p1 = vec3(polynomialEval(xf, p), polynomialEval(yf, p), polynomialEval(zf, p));
  const p2 = vec3(polynomialEval(xf, p + DELTA_P), polynomialEval(yf, p + DELTA_P), polynomialEval(zf, p + DELTA_P));
  return getDistanceBetween(p1, p2) / DELTA_P;
}
