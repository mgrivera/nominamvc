﻿/***************************************************************************

Copyright (c) Microsoft Corporation 2011.

This code is licensed using the Microsoft Public License (Ms-PL).  The text of the license
can be found here:

http://www.microsoft.com/resources/sharedsource/licensingbasics/publiclicense.mspx

***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;

namespace OpenXmlPowerTools
{
    public class TextReplacer
    {
        private class MatchSemaphore
        {
            public int MatchId;
            public MatchSemaphore(int matchId)
            {
                MatchId = matchId;
            }
        }

        private static XObject CloneWithAnnotation(XNode node)
        {
            XElement element = node as XElement;
            if (element != null)
            {
                XElement newElement = new XElement(element.Name,
                    element.Attributes(),
                    element.Nodes().Select(n => CloneWithAnnotation(n)));
                if (element.Annotation<MatchSemaphore>() != null)
                    newElement.AddAnnotation(element.Annotation<MatchSemaphore>());
            }
            return node;
        }

        private static object SearchAndReplaceTransform(XNode node,
            string search, string replace, bool matchCase)
        {
            XElement element = node as XElement;
            if (element != null)
            {
                if (element.Name == W.p)
                {
                    string contents = element.Descendants(W.t).Select(t => (string)t).StringConcatenate();
                    if (contents.Contains(search) ||
                        (!matchCase && contents.ToUpper().Contains(search.ToUpper())))
                    {
                        XElement paragraphWithSplitRuns = new XElement(W.p,
                            element.Attributes(),
                            element.Nodes().Select(n => SearchAndReplaceTransform(n, search,
                                replace, matchCase)));
                        XElement[] subRunArray = paragraphWithSplitRuns
                            .Elements(W.r)
                            .Where(e => {
                                XElement subRunElement = e.Elements().FirstOrDefault(el => el.Name != W.rPr);
                                if (subRunElement == null)
                                    return false;
                                return W.SubRunLevelContent.Contains(subRunElement.Name);
                            })
                            .ToArray();
                        int paragraphChildrenCount = subRunArray.Length;
                        int matchId = 1;
                        foreach (var pc in subRunArray
                            .Take(paragraphChildrenCount - (search.Length - 1))
                            .Select((c, i) => new { Child = c, Index = i, }))
                        {
                            var subSequence = subRunArray.SequenceAt(pc.Index).Take(search.Length);
                            var zipped = subSequence.Zip(search, (pcp, c) => new
                            {
                                ParagraphChildProjection = pcp,
                                CharacterToCompare = c,
                            });
                            bool dontMatch = zipped.Any(z => {
                                if (z.ParagraphChildProjection.Annotation<MatchSemaphore>() != null)
                                    return true;
                                bool b;
                                if (matchCase)
                                    b = z.ParagraphChildProjection.Value != z.CharacterToCompare.ToString();
                                else
                                    b = z.ParagraphChildProjection.Value.ToUpper() != z.CharacterToCompare.ToString().ToUpper();
                                return b;
                            });
                            bool match = !dontMatch;
                            if (match)
                            {
                                foreach (var item in subSequence)
                                    item.AddAnnotation(new MatchSemaphore(matchId));
                                ++matchId;
                            }
                        }

                        // The following code is locally impure, as this is the most expressive way to write it.
                        XElement paragraphWithReplacedRuns = (XElement)CloneWithAnnotation(paragraphWithSplitRuns);
                        for (int id = 1; id < matchId; ++id)
                        {
                            List<XElement> elementsToReplace = paragraphWithReplacedRuns
                                .Elements()
                                .Where(e => {
                                    var sem = e.Annotation<MatchSemaphore>();
                                    if (sem == null)
                                        return false;
                                    return sem.MatchId == id;
                                })
                                .ToList();
                            elementsToReplace.First().AddBeforeSelf(
                                new XElement(W.r,
                                    elementsToReplace.First().Elements(W.rPr),
                                    new XElement(W.t, replace)));
                            elementsToReplace.Remove();
                        }
                        var groupedAdjacentRunsWithIdenticalFormatting =
                            paragraphWithReplacedRuns
                            .Elements()
                            .GroupAdjacent(ce =>
                            {
                                if (ce.Name != W.r)
                                    return "DontConsolidate";
                                if (ce.Elements().Where(e => e.Name != W.rPr).Count() != 1 ||
                                    ce.Element(W.t) == null)
                                    return "DontConsolidate";
                                if (ce.Element(W.rPr) == null)
                                    return "";
                                return ce.Element(W.rPr).ToString(SaveOptions.None);
                            });
                        XElement paragraphWithConsolidatedRuns = new XElement(W.p,
                            groupedAdjacentRunsWithIdenticalFormatting.Select(g =>
                                {
                                    if (g.Key == "DontConsolidate")
                                        return (object)g;
                                    string textValue = g.Select(r => r.Element(W.t).Value).StringConcatenate();
                                    XAttribute xs = null;
                                    if (textValue[0] == ' ' || textValue[textValue.Length - 1] == ' ')
                                        xs = new XAttribute(XNamespace.Xml + "space", "preserve");
                                    return new XElement(W.r,
                                        g.First().Elements(W.rPr),
                                        new XElement(W.t, xs, textValue));
                                }));
                        return paragraphWithConsolidatedRuns;
                    }
                    return element;
                }
                if (element.Name == W.r && element.Elements(W.t).Any())
                {
                    var collectionOfRuns = element.Elements()
                        .Where(e => e.Name != W.rPr)
                        .Select(e =>
                            {
                                if (e.Name == W.t)
                                {
                                    string s = (string)e;
                                    IEnumerable<XElement> collectionOfSubRuns = s.Select(c =>
                                    {
                                        XElement newRun = new XElement(W.r,
                                            element.Elements(W.rPr),
                                            new XElement(W.t,
                                                c == ' ' ?
                                                new XAttribute(XNamespace.Xml + "space", "preserve") :
                                                null, c));
                                        return newRun;
                                    });
                                    return (object)collectionOfSubRuns;
                                }
                                else
                                {
                                    XElement newRun = new XElement(W.r,
                                        element.Elements(W.rPr),
                                        e);
                                    return newRun;
                                }
                            });
                    return collectionOfRuns;
                }
                return new XElement(element.Name,
                    element.Attributes(),
                    element.Nodes().Select(n => SearchAndReplaceTransform(n,
                        search, replace, matchCase)));
            }
            return node;
        }

        private static void SearchAndReplaceInXDocument(XDocument xDocument, string search,
            string replace, bool matchCase)
        {
            XElement newRoot = (XElement)SearchAndReplaceTransform(xDocument.Root,
                search, replace, matchCase);
            xDocument.Elements().First().ReplaceWith(newRoot);
        }

        public static void SearchAndReplace(WordprocessingDocument wordDoc, string search,
            string replace, bool matchCase)
        {
            if (RevisionAccepter.HasTrackedRevisions(wordDoc))
                throw new OpenXmlPowerToolsException(
                    "Search and replace will not work with documents " +
                    "that contain revision tracking.");
            XDocument xDoc;
            xDoc = wordDoc.MainDocumentPart.DocumentSettingsPart.GetXDocument();
            if (xDoc.Descendants(W.trackRevisions).Any())
                throw new OpenXmlPowerToolsException("Revision tracking is turned on for document.");

            xDoc = wordDoc.MainDocumentPart.GetXDocument();
            SearchAndReplaceInXDocument(xDoc, search, replace, matchCase);
            wordDoc.MainDocumentPart.PutXDocument();
            foreach (var part in wordDoc.MainDocumentPart.HeaderParts)
            {
                xDoc = part.GetXDocument();
                SearchAndReplaceInXDocument(xDoc, search, replace, matchCase);
                part.PutXDocument();
            }
            foreach (var part in wordDoc.MainDocumentPart.FooterParts)
            {
                xDoc = part.GetXDocument();
                SearchAndReplaceInXDocument(xDoc, search, replace, matchCase);
                part.PutXDocument();
            }
            if (wordDoc.MainDocumentPart.EndnotesPart != null)
            {
                xDoc = wordDoc.MainDocumentPart.EndnotesPart.GetXDocument();
                SearchAndReplaceInXDocument(xDoc, search, replace, matchCase);
                wordDoc.MainDocumentPart.EndnotesPart.PutXDocument();
            }
            if (wordDoc.MainDocumentPart.FootnotesPart != null)
            {
                xDoc = wordDoc.MainDocumentPart.FootnotesPart.GetXDocument();
                SearchAndReplaceInXDocument(xDoc, search, replace, matchCase);
                wordDoc.MainDocumentPart.FootnotesPart.PutXDocument();
            }
        }
    }
}
