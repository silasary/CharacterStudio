<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml" indent="yes" />
  <xsl:template match="/RulesElement">
    <xsl:element name="{/RulesElement/@type}">
      <xsl:copy-of select="/RulesElement/@name"/>
      <xsl:copy-of select="/RulesElement/@source"/>
      <xsl:copy-of select="/RulesElement/Flavor"/>
      <xsl:for-each select ="specific">
        <xsl:choose>
          <xsl:when test="@name = 'Display' 
                       or @name = 'Power Usage'
                       or @name = 'Keywords'
                       or @name = 'Action Type'
                       or @name = 'Attack Type'
                    ">
            <xsl:element name="header">
              <xsl:copy-of select="@name"/>
              <xsl:value-of select="text()"/>
            </xsl:element>
          </xsl:when>
          <xsl:when test="@name = 'Class'
                       or @name = '_ParentFeature'
                       or @name = '_Subclasses'
                    ">
            <xsl:element name="meta">
              <xsl:copy-of select="@name"/>
              <xsl:value-of select="text()"/>
            </xsl:element>
          </xsl:when>
          <xsl:otherwise>
            <xsl:copy>
              <xsl:copy-of select="@name"/>
              <xsl:value-of select="text()"/>
            </xsl:copy>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
