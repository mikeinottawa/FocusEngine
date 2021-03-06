// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Linq;
using Xenko.Core.Assets.Quantum.Internal;
using Xenko.Core.Annotations;
using Xenko.Core.Reflection;
using Xenko.Core.Quantum;
using Xenko.Core.Quantum.References;

namespace Xenko.Core.Assets.Quantum
{
    /// <summary>
    /// A <see cref="GraphNodeLinker"/> that can link nodes of an asset to the corresponding nodes in their base.
    /// </summary>
    /// <remarks>This method will invoke <see cref="AssetPropertyGraph.FindTarget(IGraphNode, IGraphNode)"/> when linking, to allow custom links for cases such as <see cref="AssetComposite"/>.</remarks>
    public class AssetToBaseNodeLinker : AssetGraphNodeLinker
    {
        private readonly AssetPropertyGraph propertyGraph;

        public AssetToBaseNodeLinker([NotNull] AssetPropertyGraph propertyGraph)
            : base(propertyGraph.Definition)
        {
            this.propertyGraph = propertyGraph;
        }

        protected override IGraphNode FindTarget(IGraphNode sourceNode)
        {
            var defaultTarget = base.FindTarget(sourceNode);
            return propertyGraph.FindTarget(sourceNode, defaultTarget);
        }

        public override ObjectReference FindTargetReference(IGraphNode sourceNode, IGraphNode targetNode, ObjectReference sourceReference)
        {
            // Not identifiable - default applies
            if (sourceReference.Index.IsEmpty || sourceReference.ObjectValue == null)
                return base.FindTargetReference(sourceNode, targetNode, sourceReference);

            // Special case for objects that are identifiable: the object must be linked to the base only if it has the same id
            var sourceAssetNode = (AssetObjectNode)sourceNode;
            var targetAssetNode = (AssetObjectNode)targetNode;
            if (!CollectionItemIdHelper.HasCollectionItemIds(sourceAssetNode.Retrieve()))
                return null;

            // Enumerable reference: we look for an object with the same id
            var targetReference = targetAssetNode.ItemReferences;
            var sourceIds = CollectionItemIdHelper.GetCollectionItemIds(sourceNode.Retrieve());
            var targetIds = CollectionItemIdHelper.GetCollectionItemIds(targetNode.Retrieve());
            var itemId = sourceIds[sourceReference.Index.Value];
            var targetKey = targetIds.GetKey(itemId);
            return targetReference.FirstOrDefault(x => Equals(x.Index.Value, targetKey));
        }
    }
}
