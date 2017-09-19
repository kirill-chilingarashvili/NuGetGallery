﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using NuGetGallery.Areas.Admin.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NuGetGallery.Areas.Admin.Controllers
{
    public class ReservedNamespaceController : AdminControllerBase
    {
        public IReservedNamespaceService ReservedNamespaceService { get; private set; }

        protected ReservedNamespaceController() { }

        public ReservedNamespaceController(IReservedNamespaceService reservedNamespaceService)
        {
            ReservedNamespaceService = reservedNamespaceService ?? throw new ArgumentNullException(nameof(reservedNamespaceService));
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public virtual JsonResult SearchPrefix(string query)
        {
            var prefixQueries = GetPrefixesFromQuery(query);
            var foundPrefixes = ReservedNamespaceService.FindReservedNamespacesForPrefixList(prefixQueries.ToList());
            var notFoundPrefixQueries = prefixQueries.Except(foundPrefixes.Select(p => p.Value));
            var resultModel = foundPrefixes.Select(fp => new ReservedNamespaceViewModel(fp, isExisting: true));
            var notFoundPrefixes = notFoundPrefixQueries.Select(q => new ReservedNamespace(q, isSharedNamespace: false, isPrefix: true));
            resultModel = resultModel.Concat(notFoundPrefixes.Select(nfp => new ReservedNamespaceViewModel(nfp, isExisting: false)).ToList());
            var results = new ReservedNamespaceSearchResult
            {
                Prefixes = resultModel
            };

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
//        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddPrefix(ReservedNamespace prefix)
        {
            try
            {
                await ReservedNamespaceService.AddReservedNamespaceAsync(prefix);
                await ReservedNamespaceService.AddOwnerToReservedNamespaceAsync(prefix.Value, "shishirx34");
                return Json(new { success = true, message = "Prefix Added" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
//        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemovePrefix(ReservedNamespace prefix)
        {
            try
            {
                await ReservedNamespaceService.DeleteReservedNamespaceAsync(prefix.Value);
                return Json(new { success = true, message = "Prefix removed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> AddOwner(ReservedNamespace prefix, string owner)
        {
            try
            {
                await ReservedNamespaceService.AddOwnerToReservedNamespaceAsync(prefix.Value, owner);
                return Json(new { success = true, message = "Owner Added!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
    //    [ValidateAntiForgeryToken]
        public async Task<JsonResult> RemoveOwner(ReservedNamespace prefix, string owner)
        {
            try
            {
                await ReservedNamespaceService.DeleteOwnerFromReservedNamespaceAsync(prefix.Value, owner);
                return Json(new { success = true, message = "Owner removed!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static string[] GetPrefixesFromQuery(string query)
        {
            return query.Split(',', '\r', '\n',';')
                .Select(prefix => prefix.Trim())
                .Where(prefix => !string.IsNullOrEmpty(prefix)).ToArray();
        }
    }
}