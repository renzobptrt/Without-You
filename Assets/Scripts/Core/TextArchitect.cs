using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextArchitect
{
	/// <summary>A dictionary keeping tabs on all architects present in a scene. Prevents multiple architects from influencing the same text object simultaneously.</summary>
	private static Dictionary<TextMeshProUGUI, TextArchitect> activeArchitects = new Dictionary<TextMeshProUGUI, TextArchitect>();

	private string preText;
	private string targetText;

	private int charactersPerFrame = 1;
	private float speed = 1f;

	public bool skip = false;

	public bool isConstructing { get { return buildProcess != null; } }
	Coroutine buildProcess = null;

	TextMeshProUGUI tmpro;

	public TextArchitect(TextMeshProUGUI tmpro, string targetText, string preText = "", int charactersPerFrame = 1, float speed = 1f)
	{
		this.tmpro = tmpro;
		this.targetText = targetText;
		this.preText = preText;
		this.charactersPerFrame = charactersPerFrame;
		this.speed = Mathf.Clamp(speed, 1f, 300f);

		Initiate();
	}

	public void Stop()
	{
		if (isConstructing)
		{
			DialogueSystem.instance.StopCoroutine(buildProcess);
		}
		buildProcess = null;
	}

	IEnumerator Construction()
	{
		int runsThisFrame = 0;

		tmpro.text = "";
		tmpro.text += preText;

		tmpro.ForceMeshUpdate();
		TMP_TextInfo inf = tmpro.textInfo;
		int vis = inf.characterCount;

		tmpro.text += targetText;

		tmpro.ForceMeshUpdate();
		inf = tmpro.textInfo;
		int max = inf.characterCount;

		tmpro.maxVisibleCharacters = vis;

		while (vis < max)
		{
			//allow skipping by increasing the characters per frame and the speed of occurance.
			if (skip)
			{
				speed = 1;
				charactersPerFrame = charactersPerFrame < 5 ? 5 : charactersPerFrame + 3;
			}

			//reveal a certain number of characters per frame.
			while (runsThisFrame < charactersPerFrame)
			{
				vis++;
				tmpro.maxVisibleCharacters = vis;
				runsThisFrame++;
			}

			//wait for the next available revelation time.
			runsThisFrame = 0;
			yield return new WaitForSeconds(0.01f * speed);
		}

		//terminate the architect and remove it from the active log of architects.
		Terminate();
	}

	void Initiate()
	{
		//check if an architect for this text object is already running. if it is, terminate it. Do not allow more than one architect to affect the same text object at once.
		TextArchitect existingArchitect = null;
		if (activeArchitects.TryGetValue(tmpro, out existingArchitect))
			existingArchitect.Terminate();

		buildProcess = DialogueSystem.instance.StartCoroutine(Construction());
		activeArchitects.Add(tmpro, this);
	}

	/// <summary>
	/// Terminate this architect. Stops the text generation process and removes it from the cache of all active architects.
	/// </summary>
	public void Terminate()
	{
		activeArchitects.Remove(tmpro);
		if (isConstructing)
			DialogueSystem.instance.StopCoroutine(buildProcess);
		buildProcess = null;
	}

	public void ForceFinish()
    {
		tmpro.maxVisibleCharacters = tmpro.text.Length;
		Terminate();
    }
	/*
	private class ENCAPSULATED_TEXT
	{
		//tag precedes text. ending tag trails it.
		private string tag = "";
		private string endingTag = "";
		//current text is the currently built target text without tags. target text is the build target.
		private string currentText = "";
		private string targetText = "";

		public string displayText { get { return _displayText; } }
		private string _displayText = "";

		//contains elements that the encapsulator will attempt to advance to when searching for sub encapsulators.
		private string[] allSpeechAndTagsArray;
		public int speechAndTagsArrayProgress { get { return arrayProgress; } }
		private int arrayProgress = 0;

		public bool isDone { get { return _isDone; } }
		private bool _isDone = false;

		public ENCAPSULATED_TEXT encapsulator = null;
		public ENCAPSULATED_TEXT subEncapsulator = null;

		public ENCAPSULATED_TEXT(string tag, string[] allSpeechAndTagsArray, int arrayProgress)
		{
			this.tag = tag;
			GenerateEndingTag();

			this.allSpeechAndTagsArray = allSpeechAndTagsArray;
			this.arrayProgress = arrayProgress;

			if (allSpeechAndTagsArray.Length - 1 > arrayProgress)
			{
				string nextPart = allSpeechAndTagsArray[arrayProgress + 1];

				targetText = nextPart;

				//increment progress so the next attempted part is updated.
				this.arrayProgress++;
			}
		}

		void GenerateEndingTag()
		{
			endingTag = tag.Replace("<", "").Replace(">", "");

			if (endingTag.Contains("="))
			{
				endingTag = string.Format("</{0}>", endingTag.Split('=')[0]);
			}
			else
			{
				endingTag = string.Format("</{0}>", endingTag);
			}
		}

		/// <summary>
		/// Take the next step in the construction process. returns true if a step was taken. returns false if a step must be made from a lower level encapsulator.
		/// </summary>
		public bool Step()
		{
			//a completed encapsulation should not step any further. Return true so if there is an error, yielding may occur.
			if (isDone)
				return true;

			//if there is a sub encapsulator, then it must finish before this encapsulator can procede.
			if (subEncapsulator != null && !subEncapsulator.isDone)
			{
				return subEncapsulator.Step();
			}
			//this encapsulator needs to finish its text.
			else
			{
				//this encapsulator has reached the end of its text.
				if (currentText == targetText)
				{
					//if there is still more dialogue to build.
					if (allSpeechAndTagsArray.Length > arrayProgress + 1)
					{
						string nextPart = allSpeechAndTagsArray[arrayProgress + 1];
						bool isATag = ((arrayProgress + 1) & 1) != 0;

						if (isATag)
						{
							//if the tag we have just reached is the terminator for this encapsulator, close it.
							if (string.Format("<{0}>", nextPart) == endingTag)
							{
								_isDone = true;

								//update this encapsulator's encapsulator is any.
								if (encapsulator != null)
								{
									string taggedText = (tag + currentText + endingTag);
									encapsulator.currentText += taggedText;
									encapsulator.targetText += taggedText;

									//update array progress to get past the current text AND the ending tag. +2
									UpdateArrayProgress(2);
								}
							}
							//if the tag we reached is not the terminator for this encapsulator, then a sub encapsulator must be created.
							else
							{
								subEncapsulator = new ENCAPSULATED_TEXT(string.Format("<{0}>", nextPart), allSpeechAndTagsArray, arrayProgress + 1);
								subEncapsulator.encapsulator = this;

								//have the encapsulators keep up with the current progress.
								UpdateArrayProgress();
							}
						}
						//if the next part is not a tag, then this is an extension to be added to the encapsulator's target.
						else
						{
							targetText += nextPart;
							UpdateArrayProgress();
						}
					}
					//finished dialogue. Close.
					else
					{
						_isDone = true;
					}
				}
				//if there is still more text to build.
				else
				{
					currentText += targetText[currentText.Length];
					//update the display text. which means we have to update any encapsulators if this is a sub encapsulator.
					UpdateDisplay("");

					return true;//a step was taken.
				}
			}
			return false;
		}

		void UpdateArrayProgress(int val = 1)
		{
			arrayProgress += val;

			if (encapsulator != null)
				encapsulator.UpdateArrayProgress(val);
		}

		void UpdateDisplay(string subValue)
		{
			_displayText = string.Format("{0}{1}{2}{3}", tag, currentText, subValue, endingTag);

			//update an encapsulators text to show its own text and its sub encapsulator's encapsulated within its tags.
			if (encapsulator != null)
				encapsulator.UpdateDisplay(displayText);
		}
	}*/
}
