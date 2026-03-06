using System;
using System.Collections.Generic;

namespace Org.GraphStream.Stream.File.Gexf
{
    public interface IGEXFElement
    {
        enum Extension
        {
            VIZ, DYNAMICS, DATA
        }

        enum TimeFormat
        {
            INTEGER, DOUBLE, DATE, DATETIME
        }

        enum DefaultEdgeType
        {
            DIRECTED, UNDIRECTED
        }

        enum IDType
        {
            STRING, INTEGER, LONG
        }

        enum Mode
        {
            STATIC, DYNAMIC, MIXED
        }

        enum ClassType
        {
            NODE, EDGE
        }

        enum AttrType
        {
            INTEGER, LONG, DOUBLE, FLOAT, BOOLEAN, LISTSTRING
        }

        void export();
    }
}
