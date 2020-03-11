//-----------------------------------------------------------------------
// <copyright file="ObjectController.cs" company="Google Inc.">
// Copyright 2014 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleVR.HelloVR
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    using System.Collections;

    public enum TargetType { 
        InfoBox,
        Teleportation
    }

    /// <summary>Controls interactable teleporting objects in the Demo scene.</summary>
    [RequireComponent(typeof(Collider))]
    public class ObjectController : MonoBehaviour
    {
        /// <summary>
        /// The material to use when this object is inactive (not being gazed at).
        /// </summary>
        public Material inactiveMaterial;

        /// <summary>The material to use when this object is active (gazed at).</summary>
        public Material gazedAtMaterial;

        /// <summary>
        /// Indicates whether this object is used to display some information (TargetType.InfoBox) or for teleporting to a different 360 scene (TargetType.Teleportation).
        /// </summary>
        public TargetType targetType;
        public string teleportationDestination;
        public bool activateInfoBoxOnTeleport;
        
        private GameObject infoBox;
        private Text teleportationText;
        private GameObject teleportationTextPanel;
        private int teleportationSetupTime = 3;

        private Vector3 startingPosition;
        private Renderer myRenderer;

        private bool infoBoxVisible;
        private bool teleportationConfirmed;

        /// <summary>Sets this instance's GazedAt state.</summary>
        /// <param name="gazedAt">
        /// Value `true` if this object is being gazed at, `false` otherwise.
        /// </param>
        public void SetGazedAt(bool gazedAt)
        {
            // Check if the inactiveMaterial and gazedAtMaterial have been set to valid materials by the user (they shouldn't be null)
            if (inactiveMaterial != null && gazedAtMaterial != null)
            {
                // if the user is looking a the object (gazedAt is true) then set the object material to gazedAtMaterial. Else set it to inactive material
                if (gazedAt)
                {
                    myRenderer.material = gazedAtMaterial;
                    if (targetType == TargetType.InfoBox)
                    {
                        TriggerInfoBox();
                    }
                    else if (targetType == TargetType.Teleportation)
                    {
                        if (activateInfoBoxOnTeleport) {
                            TriggerInfoBox();
                        }
                        TriggerTeleportation();
                    }
                    else
                    {
                        ResetTriggerObject();
                    }

                }
                else {
                    myRenderer.material = inactiveMaterial;
                    ResetTriggerObject();
                }



                return;
            }


            void TriggerInfoBox()
            {

                if (infoBox)
                {
                    infoBox.SetActive(true);
                    Debug.Log("InfoBox activated");
                }
                else
                {
                    Debug.Log("No InfoBox attached to the Target Object");
                }
            }

            void TriggerTeleportation()
            {
                if (teleportationText != null && teleportationDestination != null)
                {
                    teleportationText.gameObject.SetActive(true);
                    teleportationTextPanel.SetActive(true);
                    StartCoroutine(UpdateTeleportationProgressCoroutine());
                    Debug.Log("Teleportation activated");
                }
                else
                {
                    Debug.Log("No Teleportation text attached");
                }
            }

            IEnumerator UpdateTeleportationProgressCoroutine()
            {
                //Print the time of when the function is first called.
                Debug.Log("Started Coroutine at timestamp : " + Time.time);

                //yield on a new YieldInstruction that waits for 5 seconds.
                for (int i = teleportationSetupTime; i >= 0; i--) {
                    teleportationText.text = "Teleporting in " + i + " seconds";
                    if (gazedAt)
                    {
                        if (i == 0) { 
                            SceneManager.LoadScene(teleportationDestination); 
                        }
                        else {
                            yield return new WaitForSeconds(1);
                        }
                    }
                    else {
                        yield return 0;
                    }
                
                }

                //After we have waited 5 seconds print the time again.
                Debug.Log("Finished Coroutine at timestamp : " + Time.time);
            }

            void ResetTriggerObject() {

                if (infoBox)
                {
                    infoBox.SetActive(false);
                    Debug.Log("InfoBox deactivated");
                }
                else
                {
                    Debug.Log("No InfoBox attached to the Target Object");
                }

                if (teleportationText) {
                    teleportationText.gameObject.SetActive(false);
                    teleportationTextPanel.SetActive(false);
                    StopAllCoroutines();
                    Debug.Log("Teleportation text deactivated");
                }
                else
                {
                    Debug.Log("No Teleportation text attached");
                }


                Debug.Log("Trigger Object has been reset");
            }
        }

        /// <summary>Resets this instance and its siblings to their starting positions.</summary>
        public void Reset()
        {
            int sibIdx = transform.GetSiblingIndex();
            int numSibs = transform.parent.childCount;
            for (int i = 0; i < numSibs; i++)
            {
                GameObject sib = transform.parent.GetChild(i).gameObject;
                sib.transform.localPosition = startingPosition;
                sib.SetActive(i == sibIdx);
            }
        }

        /// <summary>Calls the Recenter event.</summary>
        public void Recenter()
        {
#if !UNITY_EDITOR
            GvrCardboardHelpers.Recenter();
#else
            if (GvrEditorEmulator.Instance != null)
            {
                GvrEditorEmulator.Instance.Recenter();
            }
#endif  // !UNITY_EDITOR
        }

        /// <summary>Teleport this instance randomly when triggered by a pointer click.</summary>
        /// <param name="eventData">The pointer click event which triggered this call.</param>
        public void TeleportRandomly(BaseEventData eventData)
        {
            // Only trigger on left input button, which maps to
            // Daydream controller TouchPadButton and Trigger buttons.
            PointerEventData ped = eventData as PointerEventData;
            if (ped != null)
            {
                if (ped.button != PointerEventData.InputButton.Left)
                {
                    return;
                }
            }

            // Pick a random sibling, move them somewhere random, activate them,
            // deactivate ourself.
            int sibIdx = transform.GetSiblingIndex();
            int numSibs = transform.parent.childCount;
            sibIdx = (sibIdx + Random.Range(1, numSibs)) % numSibs;
            GameObject randomSib = transform.parent.GetChild(sibIdx).gameObject;

            // Move to random new location ±90˚ horzontal.
            Vector3 direction = Quaternion.Euler(
                0,
                Random.Range(-90, 90),
                0) * Vector3.forward;

            // New location between 1.5m and 3.5m.
            float distance = (2 * Random.value) + 1.5f;
            Vector3 newPos = direction * distance;

            // Limit vertical position to be fully in the room.
            newPos.y = Mathf.Clamp(newPos.y, -1.2f, 4f);
            randomSib.transform.localPosition = newPos;

            randomSib.SetActive(true);
            gameObject.SetActive(false);
            SetGazedAt(false);
        }

        private void Start()
        {
            startingPosition = transform.localPosition;
            myRenderer = GetComponent<Renderer>();

            infoBox = transform.parent.Find("Canvas").Find("InfoBox").gameObject;
            teleportationText = transform.parent.Find("Canvas").Find("TeleportationText").gameObject.GetComponent<Text>();
            teleportationTextPanel = transform.parent.Find("Canvas").Find("Panel").gameObject;

            SetGazedAt(false);
        }
    }
}
