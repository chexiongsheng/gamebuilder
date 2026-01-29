import * as three from "three.mjs";

globalThis.THREE = three;
//const require = puer.module.createRequire('');

//globalThis.THREE = require("three.cjs");

const coreScripts = [
  "debug.js.txt",
  "testing.js.txt",
  "util.js.txt",
  "math.js.txt",
  "Queue.src.js.txt",
  "threejs-overrides.js.txt",
  "sleep.js.txt",
  "serialization.js.txt",
  "HandlingActor.js.txt",
  "ModuleBehaviorDatabase.js.txt",
  "HandlerApi.js.txt",
  "ModuleBehaviorsActor.js.txt",
  "ModuleBehaviorSystem.js.txt",
  "voosMain.js.txt",
  "apiv2/apiv2.js.txt",
  "apiv2/actors/actors.js.txt",
  "apiv2/actors/attributes.js.txt",
  "apiv2/actors/cloning.js.txt",
  "apiv2/actors/memory.js.txt",
  "apiv2/actors/messages.js.txt",
  "apiv2/actors/player.js.txt",
  "apiv2/actors/properties.js.txt",
  "apiv2/hierarchy/conversions.js.txt",
  "apiv2/hierarchy/parenting.js.txt",
  "apiv2/keyboard_mouse/keyboard.js.txt",
  "apiv2/keyboard_mouse/mouse.js.txt",
  "apiv2/misc/game.js.txt",
  "apiv2/misc/math.js.txt",
  "apiv2/misc/time.js.txt",
  "apiv2/multiplayer/players.js.txt",
  "apiv2/physics/attributes.js.txt",
  "apiv2/physics/casting.js.txt",
  "apiv2/physics/collisions.js.txt",
  "apiv2/physics/presets.js.txt",
  "apiv2/physics/velocity.js.txt",
  "apiv2/player_controls/actions.js.txt",
  "apiv2/player_controls/controls.js.txt",
  "apiv2/rendering/body.js.txt",
  "apiv2/rendering/color.js.txt",
  "apiv2/rendering/visibility.js.txt",
  "apiv2/terrain/blocks.js.txt",
  "apiv2/terrain/walls.js.txt",
  "apiv2/transform/position-get.js.txt",
  "apiv2/transform/position-set.js.txt",
  "apiv2/transform/rotation-get.js.txt",
  "apiv2/transform/rotation-set.js.txt",
  "apiv2/transform/scale.js.txt",
  "apiv2/player_controls/aiming.js.txt",
  "apiv2/ui/tooltips.js.txt",
  "apiv2/ui/widgets.js.txt",
  "apiv2/multiplayer/mp_events.js.txt",
  "apiv2/misc/colors.js.txt",
  "apiv2/sfx/sfx.js.txt",
  "apiv2/ui/screen.js.txt",
  "apiv2/misc/utility.js.txt",
  "apiv2/rendering/animation.js.txt",
  "apiv2/misc/cards.js.txt",
  "apiv2/particles/particleeffects.js.txt",
  "pack-unpack.js.txt",
  "apiv2/actors/camera_light.js.txt",
  "apiv2/remote/remote.js.txt",
  "apiv2/rendering/scene.js.txt"
];

// ==================== Utility Functions ====================

globalThis.assert = function (condition, message) {
  console.assert(condition, message);
};

// ==================== Initialization ====================

export function updateAgentPostMessageFlush (request, arrayBuffer) {
	  updateAgent(request, arrayBuffer);
	  postMessageFlush(request, arrayBuffer);
	  return arrayBuffer;
}

globalThis.__voosModules = {};

globalThis.getVoosModule = function (moduleName) {
  if (!globalThis.__voosModules[moduleName]) {
    throw new Error('Module not found: ' + moduleName);
  }
  return globalThis.__voosModules[moduleName];
};

export function setGlobal(k, v) {
  globalThis[k] = v;
}

export function jsonStringify(obj) {
  return JSON.stringify(obj);
}

export function jsonParse(json) {
  return JSON.parse(json);
}

console.log('[Polyfill] V8 compatibility layer loaded');
