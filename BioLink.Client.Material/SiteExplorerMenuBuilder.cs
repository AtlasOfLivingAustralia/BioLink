﻿using System;
using System.Windows.Controls;
using BioLink.Client.Extensibility;
using BioLink.Data.Model;

namespace BioLink.Client.Material {

    public class SiteExplorerMenuBuilder {

        private static SiteExplorerNodeType NodeType(string elemType) {
            return (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), elemType);
        }

        private static SiteExplorerNodeType NodeType(SiteExplorerNodeViewModel model) {
            return NodeType(model.ElemType);
        }

        public static ContextMenu BuildForFavorite(SiteFavoriteViewModel fav, MaterialExplorer explorer) {
            if (fav == null) {
                return null;
            }

            ContextMenuBuilder builder = new ContextMenuBuilder(null);

            builder.New("Refresh").Handler(() => { explorer.Refresh(); }).End();

            builder.Separator();

            builder.New("Rename").Handler(() => { fav.IsRenaming = true; }).End();

            // A little bit of a hack to reuse the edit code...simulate a site explorer node, although its not really there...
            SiteExplorerNode model = new SiteExplorerNode();
            model.ElemID = fav.ElemID;
            model.ElemType = fav.ElemType;
            model.Name = fav.Name;
            var node = new SiteExplorerNodeViewModel(model);

            var pinnable = explorer.CreatePinnable(node);
            if (pinnable != null) {
                builder.Separator();
                builder.New("_Pin to pin board").Handler(() => { PluginManager.Instance.PinObject(pinnable); });
            }

            var mnuReports = CreateReportMenuItems(node, explorer);
            if (mnuReports.HasItems) {
                builder.Separator();
                builder.AddMenuItem(mnuReports);
            }

            SiteExplorerNodeType type = (SiteExplorerNodeType)Enum.Parse(typeof(SiteExplorerNodeType), fav.ElemType);
            if (type != SiteExplorerNodeType.SiteGroup) {
                builder.Separator();
                builder.New("Details...").Handler(() => {                    
                    switch (type) {
                        case SiteExplorerNodeType.Region: explorer.EditRegion(node);
                            break;
                        case SiteExplorerNodeType.Site: explorer.EditSite(node);
                            break;
                        case SiteExplorerNodeType.SiteVisit: explorer.EditSiteVisit(node);
                            break;
                        case SiteExplorerNodeType.Trap: explorer.EditTrap(node);
                            break;
                        case SiteExplorerNodeType.Material: explorer.EditMaterial(node);
                            break;
                        default:
                            throw new Exception("[Details] Unhandled site explorer element type: " + node.ElemType);
                    }
                }).End();
            }

            return builder.ContextMenu;
        }

        public static ContextMenu Build(SiteExplorerNodeViewModel node, MaterialExplorer explorer) {

            if (node == null) {
                return null;
            }

            ContextMenuBuilder builder = new ContextMenuBuilder(null);

            builder.New("Refresh").Handler(() => { explorer.Refresh(); }).End();

            builder.Separator();

            builder.New("Rename").Handler(() => { node.IsRenaming = true; }).End();

            var addMenu = CreateAddMenu(node, explorer);
            builder.AddMenuItem(addMenu);

            builder.Separator();

            var type = NodeType(node);

            builder.New("Delete").Handler(() => {
                switch (type) {
                    case SiteExplorerNodeType.Region: explorer.DeleteRegion(node);
                        break;
                    case SiteExplorerNodeType.SiteGroup: explorer.DeleteSiteGroup(node);
                        break;
                    case SiteExplorerNodeType.Site: explorer.DeleteSite(node);
                        break;
                    case SiteExplorerNodeType.SiteVisit: explorer.DeleteSiteVisit(node);
                        break;
                    case SiteExplorerNodeType.Trap: explorer.DeleteTrap(node);
                        break;
                    case SiteExplorerNodeType.Material: explorer.DeleteMaterial(node);
                        break;
                    default:
                        throw new Exception("[Delete] Unhandled site explorer element type: " + node.ElemType);
                }
            });

            var pinnable = explorer.CreatePinnable(node);
            if (pinnable != null) {
                builder.Separator();
                builder.New("_Pin to pin board").Handler(() => { PluginManager.Instance.PinObject(pinnable); });
            }

            var mnuReports = CreateReportMenuItems(node, explorer);
            if (mnuReports.HasItems) {
                builder.Separator();
                builder.AddMenuItem(mnuReports);
            }

            builder.Separator();
            builder.AddMenuItem(CreateFavoriteMenuItems(explorer, node));


            if (type != SiteExplorerNodeType.SiteGroup) {
                builder.Separator();
                builder.New("Details...").Handler(() => {
                    switch (type) {
                        case SiteExplorerNodeType.Region: explorer.EditRegion(node);
                            break;
                        case SiteExplorerNodeType.Site: explorer.EditSite(node);
                            break;
                        case SiteExplorerNodeType.SiteVisit: explorer.EditSiteVisit(node);
                            break;
                        case SiteExplorerNodeType.Trap: explorer.EditTrap(node);
                            break;
                        case SiteExplorerNodeType.Material: explorer.EditMaterial(node);
                            break;
                        default:
                            throw new Exception("[Details] Unhandled site explorer element type: " + node.ElemType);
                    }
                }).End();
            }

            return builder.ContextMenu;
        }

        private static MenuItem CreateFavoriteMenuItems(MaterialExplorer explorer, SiteExplorerNodeViewModel node) {
            MenuItemBuilder builder = new MenuItemBuilder();
            MenuItem add = builder.New("Add to favorites").MenuItem;
            add.Items.Add(builder.New("User specific").Handler(() => { explorer.Favorites.AddToFavorites(node, false); }).MenuItem);
            add.Items.Add(builder.New("Global").Handler(() => { explorer.Favorites.AddToFavorites(node, true); }).MenuItem);
            return add;
        }


        private static MenuItem CreateReportMenuItems(SiteExplorerNodeViewModel node, MaterialExplorer explorer) {
            var builder = new MenuItemBuilder();

            MenuItem reports = builder.New("Reports").MenuItem;
            var list = explorer.Owner.GetReportsForNode(node);
            foreach (IBioLinkReport report in list) {
                IBioLinkReport reportToExecute = report;
                reports.Items.Add(builder.New(report.Name).Handler(() => { PluginManager.Instance.RunReport(explorer.Owner, reportToExecute); }).MenuItem);
            }

            return reports;
        }


        private static MenuItem CreateAddMenu(SiteExplorerNodeViewModel viewModel, MaterialExplorer explorer) {
            var addMenu = new MenuItem();
            addMenu.Header = "Add";

            var builder = new MenuItemBuilder();

            switch (NodeType(viewModel)) {
                case SiteExplorerNodeType.Region:
                    addMenu.Items.Add(builder.New("New Region").Handler(() => { explorer.AddRegion(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Site Group").Handler(() => { explorer.AddSiteGroup(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Site").Handler(() => { explorer.AddSite(viewModel); }).MenuItem);
                    break;
                case SiteExplorerNodeType.SiteGroup:
                    addMenu.Items.Add(builder.New("New Site Group").Handler(() => { explorer.AddSiteGroup(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Site").Handler(() => { explorer.AddSite(viewModel); }).MenuItem);
                    break;
                case SiteExplorerNodeType.Site:
                    addMenu.Items.Add(builder.New("New Site Visit").Handler(() => { explorer.AddSiteVisit(viewModel); }).MenuItem);
                    addMenu.Items.Add(builder.New("New Trap").Handler(() => { explorer.AddTrap(viewModel); }).MenuItem);
                    break;
                case SiteExplorerNodeType.SiteVisit:
                    addMenu.Items.Add(builder.New("New Material").Handler(() => { explorer.AddMaterial(viewModel); }).MenuItem);
                    break;
                default:
                    break;
            }


            return addMenu;
        }
    }

}
