﻿namespace AngleSharp.Network.RequestProcessors
{
    using AngleSharp.Dom;
    using AngleSharp.Dom.Html;
    using AngleSharp.Extensions;
    using AngleSharp.Services.Styling;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    sealed class StyleSheetRequestProcessor :  BaseRequestProcessor
    {
        #region Fields

        readonly HtmlLinkElement _link;
        readonly Document _document;
        readonly IResourceLoader _loader;
        IStyleEngine _engine;

        #endregion

        #region ctor

        private StyleSheetRequestProcessor(HtmlLinkElement link, Document document, IResourceLoader loader)
            : base(loader)
        {
            _link = link;
            _document = document;
            _loader = loader;
        }

        internal static StyleSheetRequestProcessor Create(HtmlLinkElement element)
        {
            var document = element.Owner;
            var loader = document.Loader;

            return loader != null ? new StyleSheetRequestProcessor(element, document, loader) : null;
        }

        #endregion

        #region Properties

        public IStyleSheet Sheet
        {
            get;
            private set;
        }

        public IStyleEngine Engine
        {
            get { return _engine ?? (_engine = _document.Options.GetStyleEngine(LinkType)); }
        }

        public String LinkType
        {
            get { return _link.Type ?? MimeTypeNames.Css; }
        }

        #endregion

        #region Methods

        public override Task ProcessAsync(ResourceRequest request)
        {
            if (Engine != null && IsDifferentToCurrentDownloadUrl(request.Target))
            {
                CancelDownload();
                Download = DownloadWithCors(request);
                return FinishDownloadAsync();
            }

            return null;
        }

        protected override async Task ProcessResponseAsync(IResponse response)
        {
            var cancel = CancellationToken.None;
            var options = new StyleOptions(_document.Context)
            {
                Element = _link,
                IsDisabled = _link.IsDisabled,
                IsAlternate = _link.RelationList.Contains(Keywords.Alternate)
            };

            var task = _engine.ParseStylesheetAsync(response, options, cancel);
            var sheet = await task.ConfigureAwait(false);
            sheet.Media.MediaText = _link.Media ?? String.Empty;
            Sheet = sheet;
        }

        #endregion

        #region Helpers

        private IDownload DownloadWithCors(ResourceRequest request)
        {
            var setting = _link.CrossOrigin.ToEnum(CorsSetting.None);
            var behavior = OriginBehavior.Taint;
            return _loader.FetchWithCors(request, setting, behavior);
        }

        #endregion
    }
}
