// Copyright (c) Xenko contributors (https://xenko.com)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.

namespace Xenko.Physics
{
    public interface IHeightfieldHeightScaleCalculator
    {
        float Calculate(IHeightfieldHeightDescription heightDescription);
    }
}
