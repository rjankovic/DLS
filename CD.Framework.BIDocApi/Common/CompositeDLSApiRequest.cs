//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CD.DLS.API
//{
//    public enum RequestCompositionType { Serial, Parallel }
//    public class CompositeDLSApiRequest : DLSApiMessage
//    {
//        RequestCompositionType CompositionType { get; set; }
//        public List<CompositeDLSApiRequestPart> Parts { get; set; }
//    }

//    public abstract class CompositeDLSApiRequestPart : DLSApiMessage
//    {
//        public Guid OriginalRequestId { get; set; }
//        public Guid RequestPartId { get; set; }
//    }

//    public class CompositeDLSApiRequestAtomicPart<R> : CompositeDLSApiRequestPart where R : CompositeRequestResponse
//    {
//        public DLSApiRequest<R> Request { get; set; }
//    }

//    public class CompositeDLSApiRequestCompositePart : CompositeDLSApiRequestPart
//    {
//        RequestCompositionType CompositionType { get; set; }
//        public List<CompositeDLSApiRequestPart> Parts { get; set; }
//    }

//    public class CompositeRequestResponse : DLSApiMessage
//    {
//        /// <summary>
//        /// The parent request of the request to which this is a respons
//        /// </summary>
//        public Guid CoveringRequestId { get; set; }
//    }
//}
