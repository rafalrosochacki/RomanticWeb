﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using RomanticWeb.Model;

namespace RomanticWeb.JsonLd
{
    public class JsonLdProcessor:IJsonLdProcessor
    {
        internal const string Id = "@id";
        internal const string Type = "@type";
        internal const string Language = "@language";
        internal const string Value = "@value";
        internal const string Context = "@context";
        internal const string Graph = "@graph";
        
        public string FromRdf(IEnumerable<EntityQuad> dataset,bool userRdfType=false,bool useNativeTypes=false)
        {
            return GetJsonStructure(dataset).ToString();
        }

        public IEnumerable<EntityQuad> ToRdf(string json)
        {
            yield break;
        }

        public string Compact(string json,string jsonLdContext)
        {
            return json;
        }

        public string Flatten(string json,string jsonLdContext)
        {
            return json;
        }

        public string Expand(string json)
        {
            return json;
        }

        private JArray GetJsonStructure(IEnumerable<EntityQuad> quads)
        {
            var root = new JArray();
            var context = new JObject();
            var blankNodes = new List<Node>(quads.Where(quad => quad.Subject.IsBlank).Join(quads, quad => quad.Subject, quad => quad.Object, (outer, inner) => inner.Object).Distinct());
            var topLevelSubjects = quads.Where(quad => (!blankNodes.Contains(quad.Subject))).Select(x => x.Subject).Distinct();
            var distinctGrafs = quads.Where(graph=>graph.Graph!=null).Select(x => x.Graph).Distinct();


            var distinctSubjects = quads.Where(graph=>graph.Graph==null).OrderBy(x => x.Subject.IsBlank ? "_:" + x.Subject.BlankNode.ToString() 
                                                                                                        : x.Subject.ToString()).Select(x => x.Subject).Distinct();
            var serialized = distinctSubjects.Select(subject => SerializeEntity(distinctGrafs,subject, context, quads, null)).ToList();
            root = new JArray(serialized);
            if (distinctGrafs.Count() > 1)
            { 
               foreach(var dGraph in distinctGrafs)
               {
                   var graphDistinctSubjects = quads.Where(graph => graph.Graph == dGraph).OrderBy(x => x.Subject.IsBlank ? "_:" + x.Subject.BlankNode.ToString() 
                                                                                                                    : x.Subject.ToString()).Select(x => x.Subject).Distinct();
                   var graphSerialized = graphDistinctSubjects.Select(subject => SerializeEntity(distinctGrafs, subject, context, quads, dGraph)).ToList();
                   JObject resultGraph = new JObject(new JProperty("@graph", graphSerialized));
                   root.Add(resultGraph);
               }
            }
            
            //foreach (var gr in distinctGrafs)
            //{
                
                //////

                ////root[Context] = context;
                //if (gr != null)
                //{
                //    root[gr.ToString()]=new JArray(serialized)
                //}
            //}

            return root;        
        }

        private JObject SerializeEntity(IEnumerable<Node> distinctGrafs, Node subject, JObject context, IEnumerable<EntityQuad> quads, Node graphName)
        {
            var groups = from quad in quads
                         where quad.Subject == subject && quad.Graph == graphName 
                         group quad.Object by quad.Predicate into g
                         select new
                         {
                             Predicate = g.Key,
                             Objects = g
                         };

            JObject result = new JObject();
            int i=0;
                        
            foreach (var g in groups)
            {
                JProperty res = g.Predicate.ToString() == "http://www.w3.org/1999/02/22-rdf-syntax-ns#type" ?
                            new JProperty(
                                new JProperty(Type,
                                    new JArray(
                                         from o in g.Objects
                                         select o.ToString())))
                :
                            new JProperty(
                                new JProperty(g.Predicate.ToString(),
                                    new JArray(
                                         from o in g.Objects
                                         select ReturnProperties(g.Predicate,o))));

                if (i==0)   
                {
                    result.AddFirst(new JProperty(Id, subject.IsBlank ? "_:" + subject.BlankNode.ToString() : subject.ToString()));
                    i++;

                    if (graphName == null)
                    {
                        if (distinctGrafs.Where(Graf => Graf.ToString() == subject.ToString()).Count() > 0)
                        {
                            List<JObject> localSerialized = new List<JObject>();
                            var localDistinctSubjects = quads.Where(graph => graph.Graph == subject).OrderBy(x => x.Subject.IsBlank ? "_:" + x.Subject.BlankNode.ToString()
                                                                                                 : x.Subject.ToString()).Select(x => x.Subject).Distinct();
                            localSerialized = localDistinctSubjects.Select(sub => SerializeEntity(distinctGrafs, sub, context, quads, subject)).ToList();
                            JProperty Graph = new JProperty("@graph", localSerialized);
                            result.Add(Graph);
                        }
                    }
                }

                result.Add(res);
            }

        return result;
        }

        private JObject ReturnProperties(Node Predicate,Node Object)
        {
                JObject returnObject = new JObject();
                JProperty Id = new JProperty(Object.IsLiteral ? "@value" : "@id", Object.IsBlank ? "_:" + Object.BlankNode.ToString() :
                                                                            (Object.IsUri ? Object.ToString() : Object.ToString().Replace("\"", "")));
                returnObject.Add(Id);

                if (Object.IsLiteral)
                {
                    if (Object.Language != null)
                    {
                        JProperty Language = new JProperty("@language", Object.Language.ToString().Replace("\"", ""));
                        returnObject.Add(Language);
                    }

                    if (Object.DataType != null)
                    {
                        JProperty Type = new JProperty("@type", Object.DataType.ToString().Replace("\"", ""));
                        returnObject.Add(Type);
                    }
                } 

            return returnObject;
        }

        private string GetJsonPropertyForPredicate(JObject context, Node node)
        {
            return node.Uri.ToString();
        }

        private JArray WrapChildrenInArray(IEnumerable<JToken> children)
        {
            return new JArray(children.ToArray());
        }
    }
}
