﻿using PDDLSharp.Models.PDDL;
using PDDLSharp.Models.PDDL.Domain;
using PDDLSharp.Models.PDDL.Expressions;

namespace MetaActionGenerator
{
    public abstract class BaseMetaGenerator : ICandidateGenerator
    {
        public abstract List<ActionDecl> Generate(List<ActionDecl> actions);

        internal bool RemoveMe(INode node, string name)
        {
            if (node is NameExp named)
            {
                return named.Name == name;
            }
            else if (node is PredicateExp pred)
            {
                if (pred.Name == name)
                    return true;
                foreach (var arg in pred.Arguments)
                    if (RemoveMe(arg, name))
                        return true;
                return false;
            }
            else if (node is NotExp not)
            {
                return RemoveMe(not.Child, name);
            }
            else if (node is AndExp and)
            {
                List<IExp> newChildren = new List<IExp>();
                foreach (var child in and.Children)
                    if (!RemoveMe(child, name))
                        newChildren.Add(child);
                and.Children = newChildren;
                return and.Children.Count > 0;
            }
            return false;
        }

        internal void RemoveUnusedParameters(ActionDecl action)
        {
            List<NameExp> newParams = new List<NameExp>();
            foreach (var arg in action.Parameters.Values)
                if (action.FindNames(arg.Name).Count > 1)
                    newParams.Add(arg);
            action.Parameters.Values = newParams;
        }
    }
}
