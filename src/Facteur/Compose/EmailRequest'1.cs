using System;
using System.Collections.Generic;
using System.Text;

namespace Facteur
{
    public class EmailRequest<T> : EmailRequest
    {
        public T Model { get; set; }
    }
}
