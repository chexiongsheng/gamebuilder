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

export const PROPS = [
  propCardTargetActor("WithWhom"),
  propString("SpeakerName"),
  propColor("SpeakerColor", "#00ff00"),
  propString("Text", "Hello", {
    label: "Text:"
  }),
  propNumber("Speed", 20),
  propString("Reply1", "Yes, please!"),
  propString("Message1", "RepliedYes"),
  propString("Reply2", "No, thanks!"),
  propString("Message2", "RepliedNo"),
  propString("Reply3", "Not sure."),
  propString("Message3", "RepliedNotSure"),
];

/**
 * @param {GActionMessage} actionMessage 
 */
export function onAction(actionMessage) {
  const target = getCardTargetActor("WithWhom", actionMessage);
  if (!target) return;
  const replies = [];
  maybeAddReply(replies, getProps().Message1, getProps().Reply1);
  maybeAddReply(replies, getProps().Message2, getProps().Reply2);
  maybeAddReply(replies, getProps().Message3, getProps().Reply3);

  send(target, "LaunchDialogue", {
    requester: myself(),
    speaker: getProps().SpeakerName || getDisplayName(),
    color: getProps().SpeakerColor,
    text: getProps().Text.replace(/\\n/g, "\n").replace("\r", ""),
    cps: getProps().Speed,
    replies: replies
  });
}

function maybeAddReply(replies, message, text) {
  if (text.trim() === "") return;
  replies.push({
    text: text,
    message: message.trim() === "" ? "Replied" : message
  });
}