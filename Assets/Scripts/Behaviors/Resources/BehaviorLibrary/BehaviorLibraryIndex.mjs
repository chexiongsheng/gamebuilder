// ActionCards
import * as addVelocityCard from "BehaviorLibrary/ActionCards/Add Velocity Card.mjs"
import * as alightCard from "BehaviorLibrary/ActionCards/Alight Card.mjs"
import * as autoPilotSetDestination from "BehaviorLibrary/ActionCards/AutoPilotSetDestination.mjs"
import * as boardCard from "BehaviorLibrary/ActionCards/Board Card.mjs"
import * as broadcastMessageActionCard from "BehaviorLibrary/ActionCards/BroadcastMessageActionCard.mjs"
import * as cameraShakeActionCard from "BehaviorLibrary/ActionCards/CameraShakeActionCard.mjs"
import * as causeDamageCard from "BehaviorLibrary/ActionCards/Cause Damage Card.mjs"
import * as changeCameraActionCard from "BehaviorLibrary/ActionCards/Change Camera Action Card.mjs"
import * as changeTint from "BehaviorLibrary/ActionCards/Change Tint.mjs"
import * as changeVariable from "BehaviorLibrary/ActionCards/Change Variable.mjs"
import * as changeColor from "BehaviorLibrary/ActionCards/ChangeColor.mjs"
import * as debugAction from "BehaviorLibrary/ActionCards/DebugAction.mjs"
import * as destroySelfActionCard from "BehaviorLibrary/ActionCards/Destroy Self Action Card.mjs"
import * as dialogueActionCard from "BehaviorLibrary/ActionCards/DialogueActionCard.mjs"
import * as endGameLOSECard from "BehaviorLibrary/ActionCards/End Game LOSE Card.mjs"
import * as endGameWINCard from "BehaviorLibrary/ActionCards/End Game WIN Card.mjs"
import * as explodeActionCard from "BehaviorLibrary/ActionCards/ExplodeActionCard.mjs"
import * as fireProjectileAtCard from "BehaviorLibrary/ActionCards/Fire Projectile At Card.mjs"
import * as fireProjectileCard from "BehaviorLibrary/ActionCards/Fire Projectile Card.mjs"
import * as fireProjectileAtGroupCard from "BehaviorLibrary/ActionCards/FireProjectileAtGroupCard.mjs"
import * as goOnstage from "BehaviorLibrary/ActionCards/Go Onstage.mjs"
import * as grabAction from "BehaviorLibrary/ActionCards/Grab Action.mjs"
import * as healDamageCard from "BehaviorLibrary/ActionCards/Heal Damage Card.mjs"
import * as hideActionCard from "BehaviorLibrary/ActionCards/Hide Action Card.mjs"
import * as jumpCard from "BehaviorLibrary/ActionCards/Jump Card.mjs"
import * as lightOffAction from "BehaviorLibrary/ActionCards/Light Off Action.mjs"
import * as lightOnAction from "BehaviorLibrary/ActionCards/Light On Action.mjs"
import * as moveAction from "BehaviorLibrary/ActionCards/Move Action.mjs"
import * as playAnimationAction from "BehaviorLibrary/ActionCards/Play Animation Action.mjs"
import * as playSound from "BehaviorLibrary/ActionCards/Play Sound.mjs"
import * as removeFromStage from "BehaviorLibrary/ActionCards/Remove From Stage.mjs"
import * as resetGameCard from "BehaviorLibrary/ActionCards/Reset Game Card.mjs"
import * as returnToSpawnPointAction from "BehaviorLibrary/ActionCards/Return To Spawn Point Action.mjs"
import * as returnToCheckpointActionCard from "BehaviorLibrary/ActionCards/ReturnToCheckpointActionCard.mjs"
import * as reviveAction from "BehaviorLibrary/ActionCards/Revive Action.mjs"
import * as saySomethingAction from "BehaviorLibrary/ActionCards/Say Something Action.mjs"
import * as scorePointAction from "BehaviorLibrary/ActionCards/Score Point Action.mjs"
import * as sendMessage from "BehaviorLibrary/ActionCards/SendMessage.mjs"
import * as sendMessageActionCard2 from "BehaviorLibrary/ActionCards/SendMessageActionCard2.mjs"
import * as sendMessageRandomActionCard from "BehaviorLibrary/ActionCards/SendMessageRandomActionCard.mjs"
import * as setCheckpointCard from "BehaviorLibrary/ActionCards/Set Checkpoint Card.mjs"
import * as showTextAction from "BehaviorLibrary/ActionCards/ShowTextAction.mjs"
import * as spawnParticleEffect from "BehaviorLibrary/ActionCards/Spawn Particle Effect.mjs"
import * as spawnActorActionCard from "BehaviorLibrary/ActionCards/SpawnActorActionCard.mjs"
import * as spawnAtIntervals from "BehaviorLibrary/ActionCards/SpawnAtIntervals.mjs"
import * as teleportAction from "BehaviorLibrary/ActionCards/Teleport Action.mjs"
import * as transferPlayerAction from "BehaviorLibrary/ActionCards/Transfer Player Action.mjs"
import * as useGrabbedItemActionCard from "BehaviorLibrary/ActionCards/UseGrabbedItemActionCard.mjs"

// CameraCards
import * as threePCameraCard from "BehaviorLibrary/CameraCards/3P Camera Card.mjs"
import * as fpsCameraCard from "BehaviorLibrary/CameraCards/FPS Camera Card.mjs"
import * as topDownCameraCard from "BehaviorLibrary/CameraCards/Top Down Camera Card.mjs"

// DeprecatedCards
import * as causeDamageSelfCard from "BehaviorLibrary/DeprecatedCards/Cause Damage Self Card.mjs"
import * as causeDamageToTargetCard from "BehaviorLibrary/DeprecatedCards/Cause Damage To Target Card.mjs"
import * as healthBarsCard from "BehaviorLibrary/DeprecatedCards/HealthBarsCard.mjs"
import * as moveOscillate from "BehaviorLibrary/DeprecatedCards/MoveOscillate.mjs"
import * as playerControlsBasicWASD from "BehaviorLibrary/DeprecatedCards/Player Controls Basic WASD.mjs"
import * as playerControlsCar from "BehaviorLibrary/DeprecatedCards/Player Controls Car.mjs"
import * as playerControlsPlane from "BehaviorLibrary/DeprecatedCards/Player Controls Plane.mjs"
import * as playerControlsPointAndClick from "BehaviorLibrary/DeprecatedCards/Player Controls Point and Click.mjs"
import * as sendMessageActionCard from "BehaviorLibrary/DeprecatedCards/SendMessageActionCard.mjs"
import * as someoneScoresXPoints from "BehaviorLibrary/DeprecatedCards/Someone Scores X Points.mjs"

// DeprecatedPanels
import * as aiControlsPanel from "BehaviorLibrary/DeprecatedPanels/AI Controls Panel.mjs"
import * as playerControlsPanel from "BehaviorLibrary/DeprecatedPanels/Player Controls Panel.mjs"
import * as winLossPanel from "BehaviorLibrary/DeprecatedPanels/Win Loss Panel.mjs"

// EventCards
import * as actorClickedEventCard from "BehaviorLibrary/EventCards/Actor Clicked Event Card.mjs"
import * as actorCountPredicateCard from "BehaviorLibrary/EventCards/Actor Count Predicate Card.mjs"
import * as collisionEventCard from "BehaviorLibrary/EventCards/Collision Event Card.mjs"
import * as gameStartEventCard from "BehaviorLibrary/EventCards/Game Start Event Card.mjs"
import * as inRangeEventCard from "BehaviorLibrary/EventCards/InRangeEventCard.mjs"
import * as killedAllActorsTagged from "BehaviorLibrary/EventCards/Killed All Actors Tagged.mjs"
import * as playerButtonEventCard from "BehaviorLibrary/EventCards/Player Button Event Card.mjs"
import * as randomPredicateCard from "BehaviorLibrary/EventCards/Random Predicate Card.mjs"
import * as receiveMessageEventCard from "BehaviorLibrary/EventCards/ReceiveMessageEventCard.mjs"
import * as scoredXPointsEventCard from "BehaviorLibrary/EventCards/Scored X Points Event Card.mjs"
import * as seesActorEventCard from "BehaviorLibrary/EventCards/SeesActorEventCard.mjs"
import * as spawnedAsCloneEventCard from "BehaviorLibrary/EventCards/Spawned As Clone Event Card.mjs"
import * as terrainCollisionEventCard from "BehaviorLibrary/EventCards/Terrain Collision Event Card.mjs"
import * as variablePredicateCard from "BehaviorLibrary/EventCards/Variable Predicate Card.mjs"

// LegacyBehaviors
import * as blink from "BehaviorLibrary/LegacyBehaviors/Blink.mjs"
import * as button from "BehaviorLibrary/LegacyBehaviors/Button.mjs"
import * as coin from "BehaviorLibrary/LegacyBehaviors/Coin.mjs"
import * as defaultBehavior from "BehaviorLibrary/LegacyBehaviors/Default Behavior.mjs"
import * as destroyClonesOnReset from "BehaviorLibrary/LegacyBehaviors/Destroy clones on reset.mjs"
import * as destroyIfUnclaimedClone from "BehaviorLibrary/LegacyBehaviors/Destroy If Unclaimed Clone.mjs"
import * as dialogBox from "BehaviorLibrary/LegacyBehaviors/Dialog box.mjs"
import * as doesDamage from "BehaviorLibrary/LegacyBehaviors/Does damage.mjs"
import * as elevator from "BehaviorLibrary/LegacyBehaviors/Elevator.mjs"
//import * as fireProjectile from "BehaviorLibrary/LegacyBehaviors/FireProjectile.mjs"
import * as followNearest from "BehaviorLibrary/LegacyBehaviors/Follow nearest.mjs"
import * as follow from "BehaviorLibrary/LegacyBehaviors/Follow.mjs"
import * as grabbable from "BehaviorLibrary/LegacyBehaviors/Grabbable.mjs"
import * as grabbing from "BehaviorLibrary/LegacyBehaviors/Grabbing.mjs"
import * as gridSpawner from "BehaviorLibrary/LegacyBehaviors/Grid Spawner.mjs"
import * as hideInPlayMode from "BehaviorLibrary/LegacyBehaviors/Hide in play mode.mjs"
import * as isometricAutoRunControls from "BehaviorLibrary/LegacyBehaviors/Isometric Auto-Run Controls.mjs"
import * as key from "BehaviorLibrary/LegacyBehaviors/Key.mjs"
import * as lookAt from "BehaviorLibrary/LegacyBehaviors/Look at.mjs"
import * as mountable from "BehaviorLibrary/LegacyBehaviors/Mountable.mjs"
import * as mounting from "BehaviorLibrary/LegacyBehaviors/Mounting.mjs"
import * as moveForward from "BehaviorLibrary/LegacyBehaviors/Move forward.mjs"
import * as openableGate from "BehaviorLibrary/LegacyBehaviors/Openable gate.mjs"
import * as physics from "BehaviorLibrary/LegacyBehaviors/Physics.mjs"
import * as placeBlock from "BehaviorLibrary/LegacyBehaviors/Place block.mjs"
import * as playerControls from "BehaviorLibrary/LegacyBehaviors/Player Controls.mjs"
import * as respawning from "BehaviorLibrary/LegacyBehaviors/Respawning.mjs"
import * as runInCircles from "BehaviorLibrary/LegacyBehaviors/Run in circles.mjs"
import * as scoreOnDeath from "BehaviorLibrary/LegacyBehaviors/Score on death.mjs"
import * as scoreOrWinOnContact from "BehaviorLibrary/LegacyBehaviors/Score or win on contact.mjs"
import * as selfDestruct from "BehaviorLibrary/LegacyBehaviors/Self-destruct.mjs"
import * as sideToSide from "BehaviorLibrary/LegacyBehaviors/Side to side.mjs"
import * as snakeForward from "BehaviorLibrary/LegacyBehaviors/Snake forward.mjs"
import * as spawner from "BehaviorLibrary/LegacyBehaviors/Spawner.mjs"
import * as takesDamage from "BehaviorLibrary/LegacyBehaviors/Takes damage.mjs"
import * as team from "BehaviorLibrary/LegacyBehaviors/Team.mjs"
import * as textBox from "BehaviorLibrary/LegacyBehaviors/Text box.mjs"
import * as throwSomething from "BehaviorLibrary/LegacyBehaviors/Throw something.mjs"
import * as trampoline from "BehaviorLibrary/LegacyBehaviors/Trampoline.mjs"
import * as turnRandomly from "BehaviorLibrary/LegacyBehaviors/Turn randomly.mjs"
import * as winByPoints from "BehaviorLibrary/LegacyBehaviors/Win by points.mjs"
import * as wizardController from "BehaviorLibrary/LegacyBehaviors/Wizard Controller.mjs"

// MoveCards
import * as backAndForth from "BehaviorLibrary/MoveCards/Back And Forth.mjs"
import * as moveAutoPilot from "BehaviorLibrary/MoveCards/MoveAutoPilot.mjs"
import * as moveChase from "BehaviorLibrary/MoveCards/MoveChase.mjs"
import * as moveInOneDirection from "BehaviorLibrary/MoveCards/MoveInOneDirection.mjs"
import * as moveLookAt from "BehaviorLibrary/MoveCards/MoveLookAt.mjs"
import * as movePath from "BehaviorLibrary/MoveCards/MovePath.mjs"
import * as moveRandomWalk from "BehaviorLibrary/MoveCards/MoveRandomWalk.mjs"
import * as playerCameraLook from "BehaviorLibrary/MoveCards/Player Camera Look.mjs"
import * as playerSpeedBasedTurn from "BehaviorLibrary/MoveCards/Player Speed Based Turn.mjs"
import * as playerSpeedThrottle from "BehaviorLibrary/MoveCards/Player Speed Throttle.mjs"
import * as playerWalkCard from "BehaviorLibrary/MoveCards/Player Walk Card.mjs"
import * as spin from "BehaviorLibrary/MoveCards/Spin.mjs"
import * as turn from "BehaviorLibrary/MoveCards/Turn.mjs"

// Panels
import * as actionOnEventPanel from "BehaviorLibrary/Panels/Action on Event Panel.mjs"
import * as alwaysPanel from "BehaviorLibrary/Panels/Always Panel.mjs"
import * as boardablePanel from "BehaviorLibrary/Panels/Boardable Panel.mjs"
import * as cameraPanel from "BehaviorLibrary/Panels/Camera Panel.mjs"
import * as gameStartPanel from "BehaviorLibrary/Panels/Game Start Panel.mjs"
import * as grabPanel from "BehaviorLibrary/Panels/Grab Panel.mjs"
import * as grabbableItemPanel from "BehaviorLibrary/Panels/Grabbable Item Panel.mjs"
import * as healthPanel from "BehaviorLibrary/Panels/Health Panel.mjs"
import * as movementPanel from "BehaviorLibrary/Panels/Movement Panel.mjs"
import * as playerControlsPanelV2 from "BehaviorLibrary/Panels/Player Controls Panel v2.mjs"
import * as screenPanel from "BehaviorLibrary/Panels/Screen Panel.mjs"
import * as switchPanel from "BehaviorLibrary/Panels/Switch Panel.mjs"
import * as timerActionPanel from "BehaviorLibrary/Panels/TimerActionPanel.mjs"

// ScreenCards
import * as floatingHealthBar from "BehaviorLibrary/ScreenCards/FloatingHealthBar.mjs"
import * as healthBar from "BehaviorLibrary/ScreenCards/HealthBar.mjs"
import * as scoreBoard from "BehaviorLibrary/ScreenCards/ScoreBoard.mjs"
import * as showVariableCard from "BehaviorLibrary/ScreenCards/ShowVariableCard.mjs"
import * as staticTextCard from "BehaviorLibrary/ScreenCards/Static Text Card.mjs"

// Specs
import * as specGActionCard from "BehaviorLibrary/Specs/spec_GActionCard.mjs"
import * as specGActionMessage from "BehaviorLibrary/Specs/spec_GActionMessage.mjs"
import * as specGEvent from "BehaviorLibrary/Specs/spec_GEvent.mjs"
import * as specGEventCard from "BehaviorLibrary/Specs/spec_GEventCard.mjs"

const voosModules = {};

// ActionCards
voosModules["builtin:Add Velocity Card"] = addVelocityCard;
voosModules["builtin:Alight Card"] = alightCard;
voosModules["builtin:AutoPilotSetDestination"] = autoPilotSetDestination;
voosModules["builtin:Board Card"] = boardCard;
voosModules["builtin:BroadcastMessageActionCard"] = broadcastMessageActionCard;
voosModules["builtin:CameraShakeActionCard"] = cameraShakeActionCard;
voosModules["builtin:Cause Damage Card"] = causeDamageCard;
voosModules["builtin:Change Camera Action Card"] = changeCameraActionCard;
voosModules["builtin:Change Tint"] = changeTint;
voosModules["builtin:Change Variable"] = changeVariable;
voosModules["builtin:ChangeColor"] = changeColor;
voosModules["builtin:DebugAction"] = debugAction;
voosModules["builtin:Destroy Self Action Card"] = destroySelfActionCard;
voosModules["builtin:DialogueActionCard"] = dialogueActionCard;
voosModules["builtin:End Game LOSE Card"] = endGameLOSECard;
voosModules["builtin:End Game WIN Card"] = endGameWINCard;
voosModules["builtin:ExplodeActionCard"] = explodeActionCard;
voosModules["builtin:Fire Projectile At Card"] = fireProjectileAtCard;
voosModules["builtin:Fire Projectile Card"] = fireProjectileCard;
voosModules["builtin:FireProjectileAtGroupCard"] = fireProjectileAtGroupCard;
voosModules["builtin:Go Onstage"] = goOnstage;
voosModules["builtin:Grab Action"] = grabAction;
voosModules["builtin:Heal Damage Card"] = healDamageCard;
voosModules["builtin:Hide Action Card"] = hideActionCard;
voosModules["builtin:Jump Card"] = jumpCard;
voosModules["builtin:Light Off Action"] = lightOffAction;
voosModules["builtin:Light On Action"] = lightOnAction;
voosModules["builtin:Move Action"] = moveAction;
voosModules["builtin:Play Animation Action"] = playAnimationAction;
voosModules["builtin:Play Sound"] = playSound;
voosModules["builtin:Remove From Stage"] = removeFromStage;
voosModules["builtin:Reset Game Card"] = resetGameCard;
voosModules["builtin:Return To Spawn Point Action"] = returnToSpawnPointAction;
voosModules["builtin:ReturnToCheckpointActionCard"] = returnToCheckpointActionCard;
voosModules["builtin:Revive Action"] = reviveAction;
voosModules["builtin:Say Something Action"] = saySomethingAction;
voosModules["builtin:Score Point Action"] = scorePointAction;
voosModules["builtin:SendMessage"] = sendMessage;
voosModules["builtin:SendMessageActionCard2"] = sendMessageActionCard2;
voosModules["builtin:SendMessageRandomActionCard"] = sendMessageRandomActionCard;
voosModules["builtin:Set Checkpoint Card"] = setCheckpointCard;
voosModules["builtin:ShowTextAction"] = showTextAction;
voosModules["builtin:Spawn Particle Effect"] = spawnParticleEffect;
voosModules["builtin:SpawnActorActionCard"] = spawnActorActionCard;
voosModules["builtin:SpawnAtIntervals"] = spawnAtIntervals;
voosModules["builtin:Teleport Action"] = teleportAction;
voosModules["builtin:Transfer Player Action"] = transferPlayerAction;
voosModules["builtin:UseGrabbedItemActionCard"] = useGrabbedItemActionCard;

// CameraCards
voosModules["builtin:3P Camera Card"] = threePCameraCard;
voosModules["builtin:FPS Camera Card"] = fpsCameraCard;
voosModules["builtin:Top Down Camera Card"] = topDownCameraCard;

// DeprecatedCards
voosModules["builtin:Cause Damage Self Card"] = causeDamageSelfCard;
voosModules["builtin:Cause Damage To Target Card"] = causeDamageToTargetCard;
voosModules["builtin:HealthBarsCard"] = healthBarsCard;
voosModules["builtin:MoveOscillate"] = moveOscillate;
voosModules["builtin:Player Controls Basic WASD"] = playerControlsBasicWASD;
voosModules["builtin:Player Controls Car"] = playerControlsCar;
voosModules["builtin:Player Controls Plane"] = playerControlsPlane;
voosModules["builtin:Player Controls Point and Click"] = playerControlsPointAndClick;
voosModules["builtin:SendMessageActionCard"] = sendMessageActionCard;
voosModules["builtin:Someone Scores X Points"] = someoneScoresXPoints;

// DeprecatedPanels
voosModules["builtin:AI Controls Panel"] = aiControlsPanel;
voosModules["builtin:Player Controls Panel"] = playerControlsPanel;
voosModules["builtin:Win Loss Panel"] = winLossPanel;

// EventCards
voosModules["builtin:Actor Clicked Event Card"] = actorClickedEventCard;
voosModules["builtin:Actor Count Predicate Card"] = actorCountPredicateCard;
voosModules["builtin:Collision Event Card"] = collisionEventCard;
voosModules["builtin:Game Start Event Card"] = gameStartEventCard;
voosModules["builtin:InRangeEventCard"] = inRangeEventCard;
voosModules["builtin:Killed All Actors Tagged"] = killedAllActorsTagged;
voosModules["builtin:Player Button Event Card"] = playerButtonEventCard;
voosModules["builtin:Random Predicate Card"] = randomPredicateCard;
voosModules["builtin:ReceiveMessageEventCard"] = receiveMessageEventCard;
voosModules["builtin:Scored X Points Event Card"] = scoredXPointsEventCard;
voosModules["builtin:SeesActorEventCard"] = seesActorEventCard;
voosModules["builtin:Spawned As Clone Event Card"] = spawnedAsCloneEventCard;
voosModules["builtin:Terrain Collision Event Card"] = terrainCollisionEventCard;
voosModules["builtin:Variable Predicate Card"] = variablePredicateCard;

// LegacyBehaviors
voosModules["builtin:Blink"] = blink;
voosModules["builtin:Button"] = button;
voosModules["builtin:Coin"] = coin;
voosModules["builtin:Default Behavior"] = defaultBehavior;
voosModules["builtin:Destroy clones on reset"] = destroyClonesOnReset;
voosModules["builtin:Destroy If Unclaimed Clone"] = destroyIfUnclaimedClone;
voosModules["builtin:Dialog box"] = dialogBox;
voosModules["builtin:Does damage"] = doesDamage;
voosModules["builtin:Elevator"] = elevator;
//voosModules["builtin:FireProjectile"] = fireProjectile;
voosModules["builtin:Follow nearest"] = followNearest;
voosModules["builtin:Follow"] = follow;
voosModules["builtin:Grabbable"] = grabbable;
voosModules["builtin:Grabbing"] = grabbing;
voosModules["builtin:Grid Spawner"] = gridSpawner;
voosModules["builtin:Hide in play mode"] = hideInPlayMode;
voosModules["builtin:Isometric Auto-Run Controls"] = isometricAutoRunControls;
voosModules["builtin:Key"] = key;
voosModules["builtin:Look at"] = lookAt;
voosModules["builtin:Mountable"] = mountable;
voosModules["builtin:Mounting"] = mounting;
voosModules["builtin:Move forward"] = moveForward;
voosModules["builtin:Openable gate"] = openableGate;
voosModules["builtin:Physics"] = physics;
voosModules["builtin:Place block"] = placeBlock;
voosModules["builtin:Player Controls"] = playerControls;
voosModules["builtin:Respawning"] = respawning;
voosModules["builtin:Run in circles"] = runInCircles;
voosModules["builtin:Score on death"] = scoreOnDeath;
voosModules["builtin:Score or win on contact"] = scoreOrWinOnContact;
voosModules["builtin:Self-destruct"] = selfDestruct;
voosModules["builtin:Side to side"] = sideToSide;
voosModules["builtin:Snake forward"] = snakeForward;
voosModules["builtin:Spawner"] = spawner;
voosModules["builtin:Takes damage"] = takesDamage;
voosModules["builtin:Team"] = team;
voosModules["builtin:Text box"] = textBox;
voosModules["builtin:Throw something"] = throwSomething;
voosModules["builtin:Trampoline"] = trampoline;
voosModules["builtin:Turn randomly"] = turnRandomly;
voosModules["builtin:Win by points"] = winByPoints;
voosModules["builtin:Wizard Controller"] = wizardController;

// MoveCards
voosModules["builtin:Back And Forth"] = backAndForth;
voosModules["builtin:MoveAutoPilot"] = moveAutoPilot;
voosModules["builtin:MoveChase"] = moveChase;
voosModules["builtin:MoveInOneDirection"] = moveInOneDirection;
voosModules["builtin:MoveLookAt"] = moveLookAt;
voosModules["builtin:MovePath"] = movePath;
voosModules["builtin:MoveRandomWalk"] = moveRandomWalk;
voosModules["builtin:Player Camera Look"] = playerCameraLook;
voosModules["builtin:Player Speed Based Turn"] = playerSpeedBasedTurn;
voosModules["builtin:Player Speed Throttle"] = playerSpeedThrottle;
voosModules["builtin:Player Walk Card"] = playerWalkCard;
voosModules["builtin:Spin"] = spin;
voosModules["builtin:Turn"] = turn;

// Panels
voosModules["builtin:Action on Event Panel"] = actionOnEventPanel;
voosModules["builtin:Always Panel"] = alwaysPanel;
voosModules["builtin:Boardable Panel"] = boardablePanel;
voosModules["builtin:Camera Panel"] = cameraPanel;
voosModules["builtin:Game Start Panel"] = gameStartPanel;
voosModules["builtin:Grab Panel"] = grabPanel;
voosModules["builtin:Grabbable Item Panel"] = grabbableItemPanel;
voosModules["builtin:Health Panel"] = healthPanel;
voosModules["builtin:Movement Panel"] = movementPanel;
voosModules["builtin:Player Controls Panel v2"] = playerControlsPanelV2;
voosModules["builtin:Screen Panel"] = screenPanel;
voosModules["builtin:Switch Panel"] = switchPanel;
voosModules["builtin:TimerActionPanel"] = timerActionPanel;

// ScreenCards
voosModules["builtin:FloatingHealthBar"] = floatingHealthBar;
voosModules["builtin:HealthBar"] = healthBar;
voosModules["builtin:ScoreBoard"] = scoreBoard;
voosModules["builtin:ShowVariableCard"] = showVariableCard;
voosModules["builtin:Static Text Card"] = staticTextCard;

// Specs
voosModules["builtin:spec_GActionCard"] = specGActionCard;
voosModules["builtin:spec_GActionMessage"] = specGActionMessage;
voosModules["builtin:spec_GEvent"] = specGEvent;
voosModules["builtin:spec_GEventCard"] = specGEventCard;

export { voosModules };