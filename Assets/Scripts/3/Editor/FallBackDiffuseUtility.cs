﻿#if UNITY_EDITOR

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

#endregion

// ReSharper disable once CheckNamespace
namespace _3.FallBackDiffuseUtility
{
	public class FallBackUtility : EditorWindow
	{
		[MenuItem("Tools/3/ReplaceFallBackDiffuse", false, 990)]
		private static void ReplaceFallBackDiffuse()
		{
			if (EditorUtility.DisplayDialog("Replace FallBack \"Diffuse\" ?",
				"Are you sure you want to replace FallBack \"Diffuse\" with FallBack \"Standard\" in all shaders in your Project?",
				"Yes", "No"))
			{
				ShaderInfo[] shaders = ShaderUtil.GetAllShaderInfo();

				foreach (ShaderInfo shaderinfo in shaders)
				{
					Shader shader = Shader.Find(shaderinfo.name);
					if (AssetDatabase.GetAssetPath(shader).StartsWith("Assets") ||
					    AssetDatabase.GetAssetPath(shader).StartsWith("Packages"))
					{
						if (FindFallBackDiffuse(shader))
						{
							ReplaceFallBack(shader);
						}
					}
				}

				AssetDatabase.Refresh();
			}
		}


		public static void ReplaceFallBack(Shader shader)
		{
			int fallbackline = FindDiffuse(shader);

			string path = AssetDatabase.GetAssetPath(shader);

			string[] lines = File.ReadAllLines(path);
			int lineNum = -1;
			string[] newLines = new string[lines.Length];
			foreach (var unused in lines)
			{
				lineNum++;
				if (lineNum >= fallbackline &&
				    lineNum <= fallbackline)
				{
					newLines[lineNum] = lines[lineNum].Replace("Diffuse", "Standard");
				}
				else
				{
					newLines[lineNum] = lines[lineNum];
				}
			}

			File.WriteAllLines(path, newLines);
		}

		private static bool FindFallBackDiffuse(Shader shader)
		{
			string filePath = AssetDatabase.GetAssetPath(shader);
			string[] shaderLines = File.ReadAllLines(filePath);
			bool fallThrough = true;
			bool found = false;
			foreach (string xline in new CommentFreeIterator(shaderLines))
			{
				string line = xline;
				int lineSkip = 0;

				while (fallThrough)
				{
					//Debug.Log("Looking for state " + state + " on line " + lineNum);
					fallThrough = false;
					lineSkip = 0; // ???
				}

				if (found)
				{
					int diffuse = line.IndexOf("Diffuse", lineSkip, StringComparison.Ordinal);
					if (diffuse != -1)
					{
						return true;
					}
				}


				int fallBack = line.IndexOf("FallBack", lineSkip, StringComparison.Ordinal);
				if (fallBack != -1)
				{
					found = true;
					if (line.IndexOf("Diffuse", StringComparison.Ordinal) != -1) return true;
				}
			}

			return false;
		}

		private static int FindDiffuse(Shader shader)
		{
			string filePath = AssetDatabase.GetAssetPath(shader);
			int lineNum = -1;
			string[] shaderLines = File.ReadAllLines(filePath);
			bool fallThrough = true;
			bool found = false;
			foreach (string xline in new CommentFreeIterator(shaderLines))
			{
				string line = xline;
				lineNum++;
				int lineSkip = 0;

				while (fallThrough)
				{
					//Debug.Log("Looking for state " + state + " on line " + lineNum);
					fallThrough = false;
					lineSkip = 0; // ???
				}

				if (found)
				{
					int diffuse = line.IndexOf("Diffuse", lineSkip, StringComparison.Ordinal);
					if (diffuse != -1)
					{
						return lineNum;
					}
				}

				int fallBack = line.IndexOf("FallBack", lineSkip, StringComparison.Ordinal);
				if (fallBack != -1)
				{
					found = true;
					if (line.IndexOf("Diffuse", StringComparison.Ordinal) != -1) return lineNum;
				}
			}

			return -1;
		}
	}

	public class CommentFreeIterator : IEnumerable<string>
	{
		private readonly IEnumerable<string> _sourceLines;

		public CommentFreeIterator(IEnumerable<string> sourceLines)
		{
			_sourceLines = sourceLines;
		}

		public IEnumerator<string> GetEnumerator()
		{
			int comment = 0;
			foreach (string xline in _sourceLines)
			{
				string line = ParserRemoveComments(xline, ref comment);
				yield return line;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public static string ParserRemoveComments(string line, ref int comment)
		{
			int lineSkip = 0;
			bool cisOpenQuote = false;


			while (true)
			{
				//Debug.Log ("Looking for comment " + lineNum);
				int openQuote = line.IndexOf("\"", lineSkip, StringComparison.CurrentCulture);
				if (cisOpenQuote)
				{
					if (openQuote == -1)
					{
						//Debug.Log("C-Open quote ignore " + lineSkip);
						break;
					}

					lineSkip = openQuote + 1;
					bool esc = false;
					int i = lineSkip - 1;
					while (i > 0 && line[i] == '\\')
					{
						esc = !esc;
						i--;
					}

					if (!esc)
					{
						cisOpenQuote = false;
					}

					//Debug.Log("C-Open quote end " + lineSkip);
					continue;
				}

				//Debug.Log ("Looking for comment " + lineSkip);
				int commentIdx;
				if (comment == 1)
				{
					commentIdx = line.IndexOf("*/", lineSkip, StringComparison.CurrentCulture);
					if (commentIdx != -1)
					{
						line = new string(' ', commentIdx + 2) + line.Substring(commentIdx + 2);
						lineSkip = commentIdx + 2;
						comment = 0;
					}
					else
					{
						line = "";
						break;
					}
				}

				commentIdx = line.IndexOf("//", lineSkip, StringComparison.CurrentCulture);
				int commentIdx2 = line.IndexOf("/*", lineSkip, StringComparison.CurrentCulture);
				if (commentIdx2 != -1 && (commentIdx == -1 || commentIdx > commentIdx2))
				{
					commentIdx = -1;
				}

				if (openQuote != -1 && (openQuote < commentIdx || commentIdx == -1) &&
				    (openQuote < commentIdx2 || commentIdx2 == -1))
				{
					cisOpenQuote = true;
					lineSkip = openQuote + 1;
					//Debug.Log("C-Open quote start " + lineSkip);
					continue;
				}

				if (commentIdx != -1)
				{
					line = line.Substring(0, commentIdx);
					break;
				}

				commentIdx = commentIdx2;
				if (commentIdx != -1)
				{
					int endCommentIdx = line.IndexOf("*/", lineSkip, StringComparison.CurrentCulture);
					if (endCommentIdx != -1)
					{
						line = line.Substring(0, commentIdx) + new string(' ', endCommentIdx + 2 - commentIdx) +
						       line.Substring(endCommentIdx + 2);
						lineSkip = endCommentIdx + 2;
					}
					else
					{
						line = line.Substring(0, commentIdx);
						comment = 1;
						break;
					}
				}
				else
				{
					break;
				}
			}

			return line;
		}
	}
}
#endif