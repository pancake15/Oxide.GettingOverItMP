﻿using System;
using System.Collections.Generic;
using Lidgren.Network;
using Oxide.Core;
using UnityEngine;

namespace Oxide.GettingOverItMP.Components
{
    public class ScoreboardUI : MonoBehaviour
    {
        private Client client;
        private PlayerControl control;
        private ChatUI chat;
        private GameObject waterObject;

        private static readonly Vector2 scoreboardSize = new Vector2(600, 400);
        private static readonly Color backgroundColor = new Color(0, 0, 0, 0.85f);
        private static GUIStyle backgroundStyle;
        private static GUIStyle serverNameStyle;
        private static GUIStyle rightAlignedLabel;
        private static GUIStyle rowStyle;

        private Vector2 scrollPosition;
        private bool mouseVisible;
        private List<MPBasePlayer> players = new List<MPBasePlayer>();

        private void Start()
        {
            client = GameObject.Find("GOIMP.Client").GetComponent<Client>() ?? throw new NotImplementedException("Could not find Client");
            control = GameObject.Find("Player").GetComponent<PlayerControl>() ?? throw new NotImplementedException("Could not find PlayerControl");

            GameObject uiObject = GameObject.Find("GOIMP.UI") ?? throw new NotImplementedException("Could not find Player object");
            chat = uiObject.GetComponent<ChatUI>() ?? throw new NotImplementedException("Could not find ChatUI");

            players.Add(control.gameObject.GetComponent<LocalPlayer>() ?? throw new NotImplementedException("Could not find LocalPlayer"));

            waterObject = GameObject.Find("Splashes") ?? throw new NotImplementedException("Could not find Splashes");

            client.PlayerJoined += (sender, args) => players.Add(args.Player);
            client.PlayerLeft += (sender, args) => players.Remove(args.Player);
        }

        private void FixedUpdate()
        {
            if (players.Count > 1)
                players.Sort((pl1, pl2) => pl2.transform.position.y.CompareTo(pl1.transform.position.y));
        }

        private void OnGUI()
        {
            if (backgroundStyle == null)
            {
                backgroundStyle = GUI.skin.box;
                backgroundStyle.normal.background = Texture2D.whiteTexture;

                serverNameStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };

                rightAlignedLabel = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperRight
                };

                rowStyle = new GUIStyle();
                rowStyle.normal.background = Texture2D.whiteTexture;
            }

            if (client.Status != NetConnectionStatus.Connected)
            {
                mouseVisible = false;
                return;
            }

            if (control.IsPaused() || chat.Writing)
            {
                mouseVisible = false;
                return;
            }

            var currentEvent = UnityEngine.Event.current;

            if (currentEvent.isKey && currentEvent.type == EventType.KeyUp && currentEvent.keyCode == KeyCode.Tab)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                mouseVisible = false;
                control.PauseInput(0);
            }

            if (!Input.GetKey(KeyCode.Tab))
                return;

            if (currentEvent.isMouse && currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
            {
                if (!mouseVisible)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    control.PauseInput(float.MinValue);
                    mouseVisible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    control.PauseInput(0);
                    mouseVisible = false;
                }
            }

            Rect screenRect = new Rect(Screen.width / 2f - scoreboardSize.x / 2f, 100, scoreboardSize.x, scoreboardSize.y);

            Color oldBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = backgroundColor;
            GUILayout.BeginArea(screenRect, backgroundStyle);
            {
                GUILayout.Label(client.ServerInfo.Name, serverNameStyle, GUILayout.Width(scoreboardSize.x));

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(8);
                    GUILayout.Label("Player", GUILayout.Width(scoreboardSize.x - 150));
                    GUILayout.Label("Height", rightAlignedLabel, GUILayout.Width(100));
                }
                GUILayout.EndHorizontal();
                
                GUILayout.BeginArea(new Rect(10, 50, scoreboardSize.x - 20, scoreboardSize.y - 55));
                {
                    var oldBackgroundColor2 = GUI.backgroundColor;
                    GUI.backgroundColor = Color.white;
                    scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);
                    GUI.backgroundColor = oldBackgroundColor2;
                    {
                        for (var i = 0; i < players.Count; i++)
                        {
                            var player = players[i];
                            GUILayout.Space(-2);
                            DrawRow(player, i);
                        }
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndArea();
            }
            GUILayout.EndArea();
            GUI.backgroundColor = oldBackgroundColor;
        }

        private void DrawRow(MPBasePlayer player, int index)
        {
            var oldBackgroundColor = GUI.backgroundColor;

            float rgb = index % 2 == 0 ? 0.3f : 0.2f;
            GUI.backgroundColor = new Color(rgb, rgb, rgb, 0.2f);

            GUILayout.BeginHorizontal(rowStyle, GUILayout.Width(scoreboardSize.x - 41));
            {
                GUILayout.Label(player.PlayerName, GUILayout.Width(scoreboardSize.x - 157));
                GUILayout.Label($"{(player.transform.position.y - waterObject.transform.position.y):0}m", rightAlignedLabel, GUILayout.Width(100));
            }
            GUILayout.EndHorizontal();

            GUI.backgroundColor = oldBackgroundColor;
        }
    }
}