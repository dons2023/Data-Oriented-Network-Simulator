from rest_framework.urls import url
from django.conf import settings
from django.contrib import admin
from django.contrib.staticfiles.urls import staticfiles_urlpatterns
from django.urls import path, include
import topology.equipments.views as equipments_views


urlpatterns = [
    path('admin/', admin.site.urls),
    url(r'^api/getNetData/$', equipments_views.NetInfoView.as_view()),
    url(r'^api/getEquipmentById/$', equipments_views.EquipInfoView.as_view()),
    url(r'^api/getNetLineOverRate/$', equipments_views.NetLineInfoOverRateView.as_view()),
    url(r'^api/getNetLineById/$', equipments_views.NetLineInfoView.as_view()),
    url(r'^api/addEquipment/$', equipments_views.AddEquipView.as_view()),
    url(r'^api/deleteEquipment/$', equipments_views.DeleteEquipView.as_view()),
    url(r'^api/updateEquipment/$', equipments_views.UpdateEquipView.as_view()),
    url(r'^api/addNetLine/$', equipments_views.AddNetLineView.as_view()),
    url(r'^api/deleteNetLine/$', equipments_views.DeleteNetLineView.as_view()),
    url(r'^api/updateNetLine/$', equipments_views.UpdateNetLineView.as_view()),
    url(r'^api/updateNetData/$', equipments_views.UpdateNetDataView.as_view()),
    url(r'^api/getFlowData/$', equipments_views.PathInfoView.as_view()),
    url(r'^api/getFlowById/$', equipments_views.PathSearchView.as_view()),
    url(r'^api/updateFlow/$', equipments_views.UpdateFlowView.as_view()),
    # url(r'^api/addFlow/$', equipments_views.AddFlowView.as_view()),
    # url(r'^api/deleteFlow/$', equipments_views.DeleteFlowView.as_view()),
    
    # Network planning modules
    url(r'^api/npaddLink/$', equipments_views.NpAddLink.as_view()),
    url(r'^api/npCheckLink/$', equipments_views.NpCheckLink.as_view()),
    url(r'^api/NpCheckLinkPredict/$', equipments_views.NpCheckLinkPredict.as_view()),
    
    # url(r'^api/withdrawnpaddNetLine/$', equipments_views.WithdrawAddNetLine.as_view()),
    url(r'^api/confirmnpaddLink/$', equipments_views.ConfirmNpAddLink.as_view()),
    url(r'^api/npdeleteLink/$', equipments_views.NpDeleteLink.as_view()),
    url(r'^api/confirmnpdeleteLink/$', equipments_views.ConfirmNpDeleteLink.as_view()),
    url(r'^api/npaddEquipment/$', equipments_views.NpAddEquipment.as_view()),
    url(r'^api/confirmnpaddEquipment/$', equipments_views.ConfirmNpAddEquipment.as_view()),
    url(r'^api/npdeleteEquipment/$', equipments_views.NpDeleteEquipment.as_view()),
    url(r'^api/confirmnpdeleteEquipment/$', equipments_views.ConfirmNpDeleteEquipment.as_view()),
    
    # Business access modules
    url(r'^api/bsAddServer/$', equipments_views.AddServerView.as_view()),
    url(r'^api/bsEditServer/$', equipments_views.UpdateServerView.as_view()),
    url(r'^api/bsDeleteServer/$', equipments_views.DeleteServerView.as_view()),
    url(r'^api/getServerData/$', equipments_views.ServerInfoView.as_view()),
    
    url(r'^api/bsBeforeIntro/$', equipments_views.SimBeforeIntro.as_view()),
    #url(r'^api/bsBeforeIntroTest/$', equipments_views.SimBeforeIntroTest.as_view()),   
     
    url(r'^api/bsAfterIntro/$', equipments_views.SimAfterIntro.as_view()),
    url(r'^api/bsAfterIntroTest/$', equipments_views.SimAfterIntroTest.as_view()),
    # test
    url(r'^api/test/$', equipments_views.Test.as_view()),

    #history
    url(r'^api/addHistory/$', equipments_views.AddHistoryView.as_view()),
    url(r'^api/getHistoriesByType/$', equipments_views.GetHistoryByTypeView.as_view()),
    url(r'^api/getHistoryById/$', equipments_views.GetHistoryByIdView.as_view()),
    url(r'^api/deleteHistoryById/$', equipments_views.DeleteHistoryByIdView.as_view()),

    url(r'^api/addlinkdata/$', equipments_views.AddlinkdataView.as_view()),



]

if settings.DEBUG:
    urlpatterns += staticfiles_urlpatterns()

