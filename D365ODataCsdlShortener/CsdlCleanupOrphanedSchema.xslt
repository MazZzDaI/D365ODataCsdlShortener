<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:edm="http://docs.oasis-open.org/odata/ns/edm"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
  >
  <xsl:output method="xml" indent="yes"/>
  <xsl:strip-space elements="*"/>

  <xsl:param name="entitySets" select="'VendorPaymentJournalLines,CustomersV2'"></xsl:param>
  <xsl:param name="setActions" select="'getDirects,RunDocumentAction,GetDocumentUriByClient'"></xsl:param>
  <xsl:param name="entityTypes" select="'CustomerV2,DimensionSet,VendorPaymentJournalLine,DimensionCombination,VoucherType,VendorPaymentJournalHeader,Currency,OfficeAddinLegalEntity,VendorPaymentMethod,VendorPaymentJournalLineSettledInvoice,VendorPaymentJournalFee,DualWriteProjectConfiguration,String,DocumentRoutingClientApp,Int32,MssLeaveRequestDate,DateTimeOffset,LeaveRequestApprovalStatus,MssLeaveTimeOffDate'"></xsl:param>
  <xsl:param name="entityFullTypes" select="'Microsoft.Dynamics.DataEntities.CustomerV2,Microsoft.Dynamics.DataEntities.DimensionSet,Microsoft.Dynamics.DataEntities.VendorPaymentJournalLine,Microsoft.Dynamics.DataEntities.DimensionCombination,Microsoft.Dynamics.DataEntities.VoucherType,Microsoft.Dynamics.DataEntities.VendorPaymentJournalHeader,Microsoft.Dynamics.DataEntities.Currency,Microsoft.Dynamics.DataEntities.OfficeAddinLegalEntity,Microsoft.Dynamics.DataEntities.VendorPaymentMethod,Microsoft.Dynamics.DataEntities.VendorPaymentJournalLineSettledInvoice,Microsoft.Dynamics.DataEntities.VendorPaymentJournalFee,Microsoft.Dynamics.DataEntities.DualWriteProjectConfiguration,Edm.String,Microsoft.Dynamics.DataEntities.DocumentRoutingClientApp,Edm.Int32,Microsoft.Dynamics.DataEntities.MssLeaveRequestDate,Edm.DateTimeOffset,Microsoft.Dynamics.DataEntities.LeaveRequestApprovalStatus,Microsoft.Dynamics.DataEntities.MssLeaveTimeOffDate'"></xsl:param>


  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>


  <xsl:template match="edm:EntityContainer">
    <xsl:copy>
      <xsl:apply-templates select="@* | edm:EntitySet[contains(concat(',', $entitySets, ','), concat(',', @Name, ','))]"/>
    </xsl:copy>
  </xsl:template>


  <xsl:template match="edm:EntityContainer/edm:EntitySet/edm:NavigationPropertyBinding" >
    <xsl:choose>
      <xsl:when test="contains(concat(',', $entitySets, ','), concat(',', ancestor::edm:EntitySet/@Name, ','))">
        <xsl:copy>
          <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
        <!-- Do nothing, effectively removing the element -->
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="edm:EntityType">
    <xsl:choose>
      <xsl:when test="contains(concat(',', $entityTypes, ','), concat(',', @Name, ','))">
        <xsl:copy>
          <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
        <!-- Do nothing, effectively removing the element -->
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="edm:NavigationProperty">
    <xsl:choose>
      <xsl:when test="contains(concat(',', $entityFullTypes, ','), concat(',', substring-before(substring-after(@Type, '('), ')'), ',')) or contains(concat(',', $entityFullTypes, ','), concat(',', @Type, ','))">
        <xsl:copy>
          <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
        <!-- Do nothing, effectively removing the element -->
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="edm:Action" >
    <xsl:choose>
      <xsl:when test="contains(concat(',', $setActions, ','), concat(',', @Name, ','))">
        <xsl:copy>
          <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
        <!-- Do nothing, effectively removing the element -->
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>


  <xsl:template match="edm:EnumType" >
    <xsl:choose>
      <xsl:when test="(concat(ancestor::edm:Schema/@Namespace,'.', @Name) = //edm:EntityType[contains(concat(',', $entityTypes, ','), concat(',', @Name, ','))]/edm:Property/@Type)
          or (concat(ancestor::edm:Schema/@Namespace,'.', @Name) = //edm:EntityType[contains(concat(',', $entityTypes, ','), concat(',', @Name, ','))]/edm:Property/edm:Annotation/@Term)">
        <xsl:copy>
          <xsl:apply-templates select="@* | node()"/>
        </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
        <!-- Do nothing, effectively removing the element -->
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>
