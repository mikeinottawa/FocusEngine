// Copyright (c) Xenko contributors (https://xenko.com) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace Xenko.Input
{
    /// <summary>
    /// A combination of <see cref="IVirtualButton"/>, by default evaluated as the operator '&&' to produce a value if all buttons are pressed.
    /// </summary>
    public class VirtualButtonGroup : Collection<IVirtualButton>, IVirtualButton
    {
        public VirtualButtonGroup(bool isDisjunction = false)
        {
            IsDisjunction = isDisjunction;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is determining the value as a disjunction ('||' operator between buttons), false by default ('&&' operator).
        /// </summary>
        /// <value><c>true</c> if this instance is disjunction; otherwise, <c>false</c>.</value>
        public bool IsDisjunction { get; set; }

        protected override void InsertItem(int index, IVirtualButton item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Cannot set null instance of VirtualButton");
            }

            if (!Contains(item))
            {
                base.InsertItem(index, item);
            }
        }

        protected override void SetItem(int index, IVirtualButton item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item", "Cannot add null instance of VirtualButton");
            }

            if (!Contains(item))
            {
                base.SetItem(index, item);
            }
        }

        /// <summary>
        /// Implements the + operator.
        /// </summary>
        /// <param name="combination">The combination.</param>
        /// <param name="virtualButton">The virtual button.</param>
        /// <returns>The result of the operator.</returns>
        public static VirtualButtonGroup operator +(VirtualButtonGroup combination, IVirtualButton virtualButton)
        {
            combination.Add(virtualButton);
            return combination;
        }

        public virtual float GetValue()
        {
            float value = 0.0f;
            foreach (var virtualButton in Items)
            {
                float newValue = virtualButton != null ? virtualButton.GetValue() : 0.0f;

                // In case of a || (disjunction) set, we return the latest non-zero value.
                if (IsDisjunction)
                {
                    if (newValue != 0.0f)
                    {
                        value = newValue;
                    }
                }
                else
                {
                    // In case of a && (conjunction) set, we return the last non-zero value unless there is a zero value.
                    if (newValue == 0.0f)
                    {
                        return 0.0f;
                    }

                    value = newValue;
                }
            }
            return value;
        }

        public override string ToString()
        {
            var text = new StringBuilder();
            for (int i = 0; i < Items.Count; i++)
            {
                var virtualButton = Items[i];
                if (i > 0)
                {
                    text.Append(IsDisjunction ? " || " : " && ");
                }
                text.AppendFormat("{0}", virtualButton);
            }
            return text.ToString();
        }

        public bool IsDown()
        {
            return CheckAnyOrAll(IsDown);
        }

        public bool IsPressed()
        {
            return CheckAnyOrAll(IsPressed);
        }

        public bool IsReleased()
        {
            return CheckAnyOrAll(IsReleased);
        }

        private bool IsDown(IVirtualButton button)
        {
            return button.IsDown();
        }

        private bool IsReleased(IVirtualButton button)
        {
            return button.IsReleased();
        }

        private bool IsPressed(IVirtualButton button)
        {
            return button.IsPressed();
        }

        private bool CheckAnyOrAll(Func<IVirtualButton, bool> check)
        {
            foreach (var virtualButton in Items)
            {
                var isDown = check(virtualButton);

                if (IsDisjunction && isDown)
                    return true;

                if (!IsDisjunction && !isDown)
                    return false;
            }

            return !IsDisjunction;
        }
    }
}
