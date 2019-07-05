﻿using System;

namespace Net.Arqsoft.QsMapper.Model
{
    /// <summary>
    /// Base class for integer based tables.
    /// </summary>
    [Serializable]
    public class IntegerBasedEntity : Entity<int>
    {
        /// <inheritdoc />
        public override bool IsNew => Id == 0;

    }
}