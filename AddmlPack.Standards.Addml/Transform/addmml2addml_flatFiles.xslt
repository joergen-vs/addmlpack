<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform version="2.0" xmlns:aml="http://www.arkivverket.no/standarder/addml"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema"
  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
  exclude-result-prefixes="xs aml">

  <!-- Forutsetninger:
       1. Korrekt datasettbeskrivelse, dvs. at den følger ADDMML 7.3 
       2. Ingen datafiler har XML-format
  -->

  <xsl:strip-space elements="*"/>

  <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>

  <!-- Globale variabler -->
  
  <xsl:variable name="datasetCharset">
    <xsl:copy-of select="/addmml/structure/dataset/charset"/>
  </xsl:variable>
  
  <xsl:variable name="datasetChardef">
    <xsl:copy-of select="/addmml/structure/dataset/chardef/concat(normalize-space(local), normalize-space(iso))"/>
  </xsl:variable>
  
  <xsl:variable name="datasetFormat">
    <xsl:copy-of select="/addmml/structure/dataset/format"/>
  </xsl:variable>
  
  <xsl:variable name="datasetRecsep">
    <xsl:copy-of select="/addmml/structure/dataset/recsep"/>
  </xsl:variable>
  

  <xsl:variable name="tempFlatFileTypes">
    <xsl:element name="flatFileTypes" namespace="http://www.arkivverket.no/standarder/addml">
      
      <xsl:variable name="localTempFlatFileTypes">
        <xsl:for-each select="//file">
          
          <xsl:variable name="format" select="if (fi_format) then fi_format else $datasetFormat"/>
          <xsl:variable name="recsep" select="if (fi_recsep) then fi_recsep else $datasetRecsep"/>
          
          <xsl:element name="tempFlatFileType" namespace="http://www.arkivverket.no/standarder/addml">
            
            <!-- tempID -->
            <xsl:element name="tempID" namespace="http://www.arkivverket.no/standarder/addml">
              <xsl:call-template name="getFlatFileTypeID">
                <xsl:with-param name="file" select="."/>
              </xsl:call-template>
            </xsl:element>
            
            <!-- charset -->
            <xsl:element name="charset" namespace="http://www.arkivverket.no/standarder/addml">
              <xsl:value-of select="if (fi_charset) then fi_charset else $datasetCharset"/>
            </xsl:element>
            
            <!-- charDefinitions -->
            <xsl:if test="fi_chardef or not($datasetChardef = '')">
              <xsl:element name="charDefinitions" namespace="http://www.arkivverket.no/standarder/addml">
                <xsl:choose>
                  <xsl:when test="fi_chardef">              
                    <xsl:for-each select="fi_chardef">
                      <xsl:element name="charDefinition" namespace="http://www.arkivverket.no/standarder/addml">
                        <xsl:attribute name="fromChar" select="fi_local"/>
                        <xsl:attribute name="toChar" select="fi_iso"/>
                      </xsl:element>
                    </xsl:for-each>
                  </xsl:when>
                  <xsl:when test="not($datasetChardef = '')">
                    <xsl:for-each select="/addmml/structure/dataset/chardef">
                      <xsl:element name="charDefinition" namespace="http://www.arkivverket.no/standarder/addml">
                        <xsl:attribute name="fromChar" select="local"/>
                        <xsl:attribute name="toChar" select="iso"/>
                      </xsl:element>
                    </xsl:for-each>
                  </xsl:when>
                </xsl:choose>
              </xsl:element>
            </xsl:if>

            <!-- format -->
            <xsl:choose>
              <xsl:when test="$format='FIXED'">
                <xsl:element name="fixedFileFormat" namespace="http://www.arkivverket.no/standarder/addml">
                  <xsl:if test="not(normalize-space($recsep)='NO')">
                    <xsl:element name="recordSeparator" namespace="http://www.arkivverket.no/standarder/addml">
                      <xsl:value-of select="$recsep"/>
                    </xsl:element>            
                  </xsl:if>
                </xsl:element>
              </xsl:when>
              <xsl:when test="$format='DELIM'">
                <xsl:element name="delimFileFormat" namespace="http://www.arkivverket.no/standarder/addml">
                  <xsl:if test="not(normalize-space($recsep)='NO')">
                    <xsl:element name="recordSeparator" namespace="http://www.arkivverket.no/standarder/addml">
                    <xsl:value-of select="$recsep"/>
                    </xsl:element>
                  </xsl:if>
                  <xsl:if test="delimspec">
                    <xsl:element name="fieldSeparatingChar" namespace="http://www.arkivverket.no/standarder/addml">
                      <xsl:value-of select="delimspec/fieldsep"/>
                    </xsl:element>
                    <xsl:if test="delimspec/quote">
                      <xsl:element name="quotingChar" namespace="http://www.arkivverket.no/standarder/addml">
                        <xsl:value-of select="delimspec/quote"/>
                      </xsl:element>
                    </xsl:if>
                  </xsl:if>
                </xsl:element>
              </xsl:when>
            </xsl:choose>
            
          </xsl:element>
        </xsl:for-each>
        
      </xsl:variable>

      <xsl:for-each-group select="$localTempFlatFileTypes//*:tempFlatFileType" group-by="*:tempID">
        <xsl:element name="tempFlatFileType" namespace="http://www.arkivverket.no/standarder/addml">
          <!-- name -->
          <xsl:attribute name="name" select="concat('fft', position())"/>
          
          <!-- Resten -->
          <xsl:copy-of select="*"/>
        </xsl:element>
      </xsl:for-each-group>

    </xsl:element>

  </xsl:variable>

  <xsl:variable name="tempFieldTypes">
    <xsl:element name="fieldTypes" namespace="http://www.arkivverket.no/standarder/addml">
      
      <xsl:variable name="localTempFieldTypes">
        <xsl:for-each select="//fieldtype">
          
          <xsl:element name="tempFieldType" namespace="http://www.arkivverket.no/standarder/addml">
            
            <!-- tempID -->
            <xsl:element name="tempID" namespace="http://www.arkivverket.no/standarder/addml">
              <xsl:call-template name="getFieldTypeID">
                <xsl:with-param name="fieldtype" select="."/>
              </xsl:call-template>
            </xsl:element>
            
            <!-- dataType -->
            <xsl:if test="datatype">
              <xsl:element name="dataType" namespace="http://www.arkivverket.no/standarder/addml">
                <xsl:value-of select="lower-case(normalize-space(datatype))"/>
              </xsl:element>
            </xsl:if> 
            
            <!-- fieldFormat -->
            <xsl:if test="mask">
              <xsl:element name="fieldFormat" namespace="http://www.arkivverket.no/standarder/addml">
                <xsl:value-of select="normalize-space(mask)"/>
              </xsl:element>
            </xsl:if> 
            
            <!-- packType -->
            <xsl:if test="packed">
              <xsl:element name="packType" namespace="http://www.arkivverket.no/standarder/addml">
                <xsl:value-of select="normalize-space(packType)"/>
              </xsl:element>
            </xsl:if> 
            
            <!-- nullValues -->
            <xsl:if test="ft_nullvalue">
              <xsl:element name="nullValues" namespace="http://www.arkivverket.no/standarder/addml">
                <xsl:element name="nullValue" namespace="http://www.arkivverket.no/standarder/addml">
                  <xsl:value-of select="normalize-space(ft_nullvalue)"/>
                </xsl:element>
              </xsl:element>
            </xsl:if> 
            
          </xsl:element>
        </xsl:for-each>
        
      </xsl:variable>
      
      <xsl:for-each-group select="$localTempFieldTypes//*:tempFieldType" group-by="*:tempID">
        <xsl:element name="tempFieldType" namespace="http://www.arkivverket.no/standarder/addml">
          <!-- name -->
          <xsl:attribute name="name" select="concat(*:dataType, '-', position())"/>
          
          <!-- Resten -->
          <xsl:copy-of select="*"/>
        </xsl:element>
      </xsl:for-each-group>
      
    </xsl:element>
      
  </xsl:variable>


  <xsl:template name="getFlatFileTypeID">
    <xsl:param name="file"/>
    
    <xsl:variable name="fi_chardef">
      <xsl:value-of select="$file//fi_chardef/concat(normalize-space(fi_local), normalize-space(fi_iso))"/>
    </xsl:variable>
    
    <!-- charset -->
    <xsl:variable name="charsetID">
      <xsl:value-of select="if ($file/fi_charset) then $file/fi_charset else $datasetCharset"/>          
    </xsl:variable>
    
    <!-- chardef -->
    <xsl:variable name="chardefID">
      <xsl:value-of select="if ($file/fi_chardef) then $fi_chardef else $datasetChardef"/>
    </xsl:variable>
    
    <!-- format -->
    <xsl:variable name="formatID">
      <xsl:value-of select="if ($file/fi_format) then $file/fi_format else $datasetFormat"/>          
    </xsl:variable>
    
    <!-- delimspec -->
    <xsl:variable name="delimspecID">
      <xsl:value-of select="$file//delimspec/concat(normalize-space(fieldsep/text()), normalize-space(quote/text()))"/>          
    </xsl:variable>
    
    <!-- recsep -->
    <xsl:variable name="recsepID">
      <xsl:value-of select="if ($file/fi_recsep) then $file/fi_recsep else $datasetRecsep"/>          
    </xsl:variable>
    
    <xsl:value-of select="concat('[', $charsetID, '][', $chardefID, '][', $formatID, '][', $delimspecID, '][', $recsepID, ']')"/>  
  </xsl:template>


  <xsl:template name="getFieldTypeID">
    <xsl:param name="fieldtype"/>
    
    <!-- datatype -->
    <xsl:variable name="datatypeID">
      <xsl:value-of select="$fieldtype/datatype"/>          
    </xsl:variable>
    
    <!-- mask -->
    <xsl:variable name="maskID">
      <xsl:value-of select="$fieldtype/mask"/>
    </xsl:variable>
    
    <!-- packed -->
    <xsl:variable name="packedID">
      <xsl:value-of select="$fieldtype/packed"/>          
    </xsl:variable>
    
    <!-- ft_nullvalue -->
    <xsl:variable name="ft_nullvalueID">
      <xsl:value-of select="$fieldtype/ft_nullvalue"/>          
    </xsl:variable>
    
    <xsl:value-of select="concat('[', $datatypeID, '][', $maskID, '][', $packedID, '][', $ft_nullvalueID, ']')"/>
  </xsl:template>
  
  <!-- ***************************** -->

  <!-- Rot-template -->
  <xsl:template match="/">
    <xsl:element name="addml" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:attribute name="xsi:schemaLocation">http://www.arkivverket.no/standarder/addml addml.xsd</xsl:attribute>

      <!-- Sjekke om ADDMML 7.3 -->
      <xsl:if test="not(normalize-space(addmml/@version) = '7.3')">
        <xsl:comment>Inn-dokumentet følger ikke ADDMML 7.3!</xsl:comment>
      </xsl:if>
      <xsl:if test="normalize-space(addmml/@version) = '7.3'">
        <xsl:apply-templates select="addmml/structure/dataset"/>
      </xsl:if>

    </xsl:element>

  </xsl:template>


  <xsl:template match="addmml/structure/dataset">

    <!-- Kan ikke konvertere hvis formatet er XML -->

    <xsl:if test="normalize-space(./format) = 'XML'">
      <xsl:comment>dataset - format kan ikke ha verdien 'XML'!</xsl:comment>
    </xsl:if>

    <xsl:if test="not(normalize-space(./format) = 'XML')">

      <xsl:element name="dataset" namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:attribute name="name">
          <xsl:value-of select="/addmml/structure/dataset/@ds_id"/>
        </xsl:attribute>

        <xsl:element name="description" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:text>Konvertert fra ADDMML 7.3 til ADDML 8.2.</xsl:text>
          <xsl:if test="ds_descr">
            <xsl:text> 
          ADDMML 7.3 - ds_descr: </xsl:text>
            <xsl:value-of select="ds_descr"/>
          </xsl:if>
        </xsl:element>
        <xsl:apply-templates select="/addmml/reference"/>
        <xsl:call-template name="flatFiles"/>

      </xsl:element>
    </xsl:if>

  </xsl:template>



  <!-- from ds_descr to description -->
  <xsl:template match="ds_descr">
    <xsl:call-template name="description"/>
  </xsl:template>



  <xsl:template match="reference">
    <xsl:element name="reference" namespace="http://www.arkivverket.no/standarder/addml">
      <!-- context -->
      <xsl:element name="context" namespace="http://www.arkivverket.no/standarder/addml">
        <!-- description med opprinnelige verdier -->
        <xsl:element name="description" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:text>Opprinnelige verdier: </xsl:text>
          <xsl:text>archive - ar_id: </xsl:text>
          <xsl:value-of select="normalize-space(archives/@ar_id)"/>
          <xsl:text>, </xsl:text>
          <xsl:text>archive - ar_name: </xsl:text>
          <xsl:value-of select="normalize-space(archives/ar_name)"/>
          <xsl:text>, </xsl:text>
          <xsl:text>system - sy_id: </xsl:text>
          <xsl:value-of select="normalize-space(system/@sy_id)"/>
          <xsl:text>, </xsl:text>
          <xsl:text>system - sy_name: </xsl:text>
          <xsl:value-of select="normalize-space(system/sy_name)"/>
        </xsl:element>
        <xsl:element name="additionalElements"
          namespace="http://www.arkivverket.no/standarder/addml">
          <!-- recordCreator -->
          <xsl:call-template name="create-additionalElement">
            <xsl:with-param name="name" select="'recordCreator'"/>
            <xsl:with-param name="value" select="'[RECORD_CREATOR]'"/>
          </xsl:call-template>
          <!-- archive -->
          <xsl:if test="normalize-space(archives/@ar_id) != '' or normalize-space(archives/ar_name) != ''">
            <xsl:call-template name="create-additionalElement">
              <xsl:with-param name="name" select="'archive'"/>
              <xsl:with-param name="value" select="concat(normalize-space(archives/@ar_id), ' - ', normalize-space(archives/ar_name))"/>
            </xsl:call-template>
          </xsl:if>
          <!-- systemName -->
          <xsl:if
            test="normalize-space(system/@sy_id) != '' or normalize-space(system/sy_name) != ''">
            <xsl:call-template name="create-additionalElement">
              <xsl:with-param name="name" select="'systemName'"/>
              <xsl:with-param name="value"
                select="concat(normalize-space(system/@sy_id), ' - ', normalize-space(system/sy_name))"
              />
            </xsl:call-template>
          </xsl:if>
          <!-- systemType -->
          <xsl:call-template name="create-additionalElement">
            <xsl:with-param name="name" select="'systemType'"/>
            <xsl:with-param name="value" select="'[SYSTEM_TYPE]'"/>
          </xsl:call-template>
        </xsl:element>
      </xsl:element>

      <!-- content -->
      <xsl:if
        test="normalize-space(system/startdate) != '' or normalize-space(system/enddate) != ''">
        <xsl:element name="content" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:element name="additionalElements"
            namespace="http://www.arkivverket.no/standarder/addml">
            <!-- startDate og endDate -->
            <xsl:element name="additionalElement"
              namespace="http://www.arkivverket.no/standarder/addml">
              <xsl:attribute name="name">archivalPeriod</xsl:attribute>
              <xsl:element name="properties"
                namespace="http://www.arkivverket.no/standarder/addml">
                <xsl:call-template name="create-property">
                  <xsl:with-param name="name" select="'startDate'"/>
                  <xsl:with-param name="value" select="system/startdate"/>
                </xsl:call-template>
                <xsl:call-template name="create-property">
                  <xsl:with-param name="name" select="'endDate'"/>
                  <xsl:with-param name="value" select="system/enddate"/>
                </xsl:call-template>
              </xsl:element>
            </xsl:element>
          </xsl:element>
        </xsl:element>
      </xsl:if>
    </xsl:element>
  </xsl:template>
  
  <!-- Flate filer -->
  <xsl:template name="flatFiles">
    <xsl:element name="flatFiles" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:for-each select="//file">
        <xsl:element name="flatFile" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:attribute name="name" select="@name"/>
          <xsl:attribute name="definitionReference" select="@name"/>
          <xsl:element name="properties" namespace="http://www.arkivverket.no/standarder/addml">
            <xsl:call-template name="create-property">
              <xsl:with-param name="name" select="'fileName'"/>
              <xsl:with-param name="value" select="@path"/>
            </xsl:call-template>
          </xsl:element>
        </xsl:element>

      </xsl:for-each>

      <xsl:call-template name="flatFileDefinitions"></xsl:call-template>
      
      <xsl:call-template name="structureTypes"></xsl:call-template>

    </xsl:element>
  </xsl:template>

  <!-- Flat fil-definisjoner -->
  <xsl:template name="flatFileDefinitions">
    
    <xsl:element name="flatFileDefinitions" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:for-each select="//file">
        <xsl:element name="flatFileDefinition" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:attribute name="name" select="@name"/>
          <xsl:attribute name="typeReference">
            <xsl:call-template name="getFlatFileTypeName">
              <xsl:with-param name="tempID">
                <xsl:call-template name="getFlatFileTypeID">
                  <xsl:with-param name="file" select="."/>
                </xsl:call-template>
              </xsl:with-param>
            </xsl:call-template>
          </xsl:attribute>
          
          <xsl:element name="recordDefinitions" namespace="http://www.arkivverket.no/standarder/addml">
            <xsl:apply-templates select="rectype"/>
          </xsl:element>
        </xsl:element>
      </xsl:for-each>
    </xsl:element>
  </xsl:template>

  <xsl:template match="rectype">
    <xsl:element name="recordDefinition" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:attribute name="name" select="@name"/>      
      
      <!-- Keys -->
      <xsl:if test="count(./primkey) > 0 or count(./altkey) > 0 or 
        count(./forkey) > 0">
        <xsl:element name="keys" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:apply-templates select="primkey"/>
          <xsl:apply-templates select="altkey"/>                    
          <xsl:apply-templates select="forkey"/>
        </xsl:element>
      </xsl:if>

      <xsl:element name="fieldDefinitions" namespace="http://www.arkivverket.no/standarder/addml">
        
        <xsl:apply-templates select="fieldtype"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="fieldtype">
    <xsl:element name="fieldDefinition" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:attribute name="name" select="@name"/>      
      <xsl:attribute name="typeReference">
        <xsl:call-template name="getFieldTypeName">
          <xsl:with-param name="tempID">
            <xsl:call-template name="getFieldTypeID">
              <xsl:with-param name="fieldtype" select="."/>
            </xsl:call-template>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:attribute>
      
      <xsl:apply-templates select="ft_descr"/>
      <xsl:apply-templates select="century"/>
      <xsl:apply-templates select="startpos"/>
      <xsl:apply-templates select="endpos"/>
      <xsl:apply-templates select="ft_fixlength"/>
      <xsl:apply-templates select="ft_minlength"/>
      <xsl:apply-templates select="ft_maxlength"/>
      <xsl:apply-templates select="unique"/>
      <xsl:apply-templates select="nonull"/>
      
      
      <xsl:if test="code">
        <xsl:element name="codes" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:apply-templates select="code"/>
        </xsl:element>
      </xsl:if>
      
    </xsl:element>
  </xsl:template>

  <!-- from ft_descr to description -->
  <xsl:template match="ft_descr">
    <xsl:call-template name="description"/>
  </xsl:template>
  
  <xsl:template match="century">
    <xsl:if test="normalize-space(.) != ''">
    <xsl:element name="properties" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:element name="property" namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:attribute name="name">century</xsl:attribute>
        <xsl:element name="properties" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:variable name="startYear">
            <xsl:value-of select="substring-before(., '-')"/>
          </xsl:variable>
          <xsl:variable name="endYear">
            <xsl:value-of select="substring-after(., '-')"/>
          </xsl:variable>
          
          <xsl:if test="$startYear = '' or $endYear = ''">
            <xsl:call-template name="create-property">
              <xsl:with-param name="name" select="'startYear'"/>
              <xsl:with-param name="value" select="concat('Error: ', .)"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:if test="$endYear != ''">
            <xsl:call-template name="create-property">
              <xsl:with-param name="name" select="'startYear'"/>
              <xsl:with-param name="value" select="$startYear"/>
            </xsl:call-template>
          </xsl:if>
          <xsl:if test="$endYear != ''">
            <xsl:call-template name="create-property">
              <xsl:with-param name="name" select="'endYear'"/>
              <xsl:with-param name="value" select="$endYear"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:element>
      </xsl:element>
    </xsl:element>
    </xsl:if>
  </xsl:template>  
  
  <xsl:template match="startpos">
    <xsl:element name="startPos" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="endpos">
    <xsl:element name="endPos" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="ft_fixlength">
    <xsl:element name="fixedLength" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="ft_minlength">
    <xsl:element name="minLength" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="ft_maxlength">
    <xsl:element name="maxLength" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="unique">
    <xsl:element name="unique" namespace="http://www.arkivverket.no/standarder/addml"/>
  </xsl:template>

  <xsl:template match="nonull">
    <xsl:element name="notNull" namespace="http://www.arkivverket.no/standarder/addml"/>
  </xsl:template>
  
  <xsl:template match="code">
    <xsl:element name="code" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:attribute name="codeValue" select="value"/>
      <xsl:attribute name="explan" select="explan"/>
    </xsl:element>  
  </xsl:template>

  <xsl:template match="altkey">
    <xsl:element name="key" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:attribute name="name" select="generate-id(.)"/>         
      <xsl:element name="alternateKey" namespace="http://www.arkivverket.no/standarder/addml"/>
      <xsl:element name="fieldDefinitionReferences" namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:call-template name="split_string_to_fieldDefinitionReference">
          <xsl:with-param name="string" select="."/>
          <xsl:with-param name="char" select="'+'"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="forkey">
    <xsl:element name="key" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:attribute name="name" select="generate-id(.)"/>         
      <xsl:element name="foreignKey" namespace="http://www.arkivverket.no/standarder/addml">
         
        <!-- reffilerec = file:rectype -->
        <xsl:element name="flatFileDefinitionReference" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:attribute name="name">
            <xsl:value-of select="substring-before(./reffilerec, ':')"/>
          </xsl:attribute>
          <xsl:element name="recordDefinitionReferences" namespace="http://www.arkivverket.no/standarder/addml">
            <xsl:element name="recordDefinitionReference" namespace="http://www.arkivverket.no/standarder/addml">
              <xsl:attribute name="name">
                <xsl:value-of select="substring-after(./reffilerec, ':')"/>
              </xsl:attribute>                         
              <xsl:element name="fieldDefinitionReferences" namespace="http://www.arkivverket.no/standarder/addml">
                <xsl:call-template name="split_string_to_fieldDefinitionReference">
                  <xsl:with-param name="string" select="./fields"/>
                  <xsl:with-param name="char" select="'+'"/>
                </xsl:call-template>
              </xsl:element>
            </xsl:element>  
          </xsl:element>
        </xsl:element>
        <xsl:apply-templates select="reltype"/>                
      </xsl:element>
      
      <xsl:element name="fieldDefinitionReferences" namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:call-template name="split_string_to_fieldDefinitionReference">
          <xsl:with-param name="string" select="./fields"/>
          <xsl:with-param name="char" select="'+'"/>
        </xsl:call-template>
      </xsl:element>
          
    </xsl:element>
  </xsl:template>   
  
  <xsl:template match="primkey">
    <xsl:element name="key" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:attribute name="name" select="generate-id(.)"/>         
      <xsl:element name="primaryKey" namespace="http://www.arkivverket.no/standarder/addml"/>
      <xsl:element name="fieldDefinitionReferences" namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:call-template name="split_string_to_fieldDefinitionReference">
          <xsl:with-param name="string" select="."/>
          <xsl:with-param name="char" select="'+'"/>
        </xsl:call-template>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  
  <xsl:template match="reltype">
    <xsl:element name="relationType" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>
  
  <xsl:template name="structureTypes">
    <xsl:element name="structureTypes" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:element name="flatFileTypes" namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:for-each select="$tempFlatFileTypes//*:tempFlatFileType">
          <xsl:element name="flatFileType" namespace="http://www.arkivverket.no/standarder/addml">
            <xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
            <xsl:copy-of select="* except *:tempID"></xsl:copy-of>
          </xsl:element>  
        </xsl:for-each>
        
      </xsl:element>  

      <!-- fieldTypes -->
      <xsl:element name="fieldTypes" namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:for-each select="$tempFieldTypes//*:tempFieldType">
          <xsl:element name="fieldType" namespace="http://www.arkivverket.no/standarder/addml">
            <xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute>
            <xsl:copy-of select="* except *:tempID"></xsl:copy-of>
          </xsl:element>  
        </xsl:for-each>
        
      </xsl:element>  
      

    </xsl:element>
  </xsl:template>



  <xsl:template name="getFlatFileTypeName">
    <xsl:param name="tempID"/>
    
    <xsl:value-of select="$tempFlatFileTypes//*:tempFlatFileType[*:tempID=$tempID]/@name"/>  
  </xsl:template>    

  <xsl:template name="getFieldTypeName">
    <xsl:param name="tempID"/>
    
    <xsl:value-of select="$tempFieldTypes//*:tempFieldType[*:tempID=$tempID]/@name"/>  
  </xsl:template>
  
  <xsl:template name="description">
    <xsl:element name="description" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>


  <!-- Oppretter et additionalElement-element med name og value -->
  <!-- Hvis name eller value er tomme, opprettes ikke elementet -->
  <!-- Elementet som opprettes her kan ikke inneholde andre elementer, -->
  <!-- f.eks. properties eller additionalElements. -->
  <xsl:template name="create-additionalElement">
    <xsl:param name="name" select="''"/>
    <xsl:param name="value" select="''"/>
    <xsl:if test="normalize-space($name) != '' and normalize-space($value) != ''">
      <xsl:element name="additionalElement"
        namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:attribute name="name">
          <xsl:value-of select="$name"/>
        </xsl:attribute>
        <xsl:element name="value" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:value-of select="$value"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <!-- Oppretter et property-element med name og value -->
  <!-- Hvis name eller value er tomme, opprettes ikke elementet -->
  <xsl:template name="create-property">
    <xsl:param name="name" select="''"/>
    <xsl:param name="value" select="''"/>
    <xsl:if test="normalize-space($name) != '' and normalize-space($value) != ''">
      <xsl:element name="property" namespace="http://www.arkivverket.no/standarder/addml">
        <xsl:attribute name="name">
          <xsl:value-of select="$name"/>
        </xsl:attribute>
        <xsl:element name="value" namespace="http://www.arkivverket.no/standarder/addml">
          <xsl:value-of select="$value"/>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="fieldDefinitionReference">
    <xsl:param name="value" select="''"/>
    <xsl:element name="fieldDefinitionReference" namespace="http://www.arkivverket.no/standarder/addml">
      <xsl:attribute name="name">
        <xsl:value-of select="$value"/>
      </xsl:attribute>
    </xsl:element>
  </xsl:template>

  <xsl:template name="split_string_to_fieldDefinitionReference">
    <xsl:param name="string" select="''"/>
    <xsl:param name="char" select="''"/>
    
    <xsl:if test="string-length($string) > 0">
      
      <xsl:variable name="fieldName" select="substring-before($string, $char)"/>
      
      <xsl:choose>
        
        <xsl:when test="string-length($fieldName) > 0">
          <xsl:call-template name="fieldDefinitionReference">
            <xsl:with-param name="value" select="$fieldName"/>
          </xsl:call-template>
          
          <xsl:variable name="rest" select="substring-after($string, $char)"/>
          
          <xsl:if test="string-length($rest) > 0">
            <xsl:call-template name="split_string_to_fieldDefinitionReference">
              <xsl:with-param name="string" select="$rest"/>
              <xsl:with-param name="char" select="$char"/>
            </xsl:call-template>
          </xsl:if>
        </xsl:when>
        
        <xsl:otherwise>
          <!-- Siste eller en fieldName -->
          <xsl:call-template name="fieldDefinitionReference">
            <xsl:with-param name="value" select="$string"/>
          </xsl:call-template>
        </xsl:otherwise>
        
      </xsl:choose>
      
    </xsl:if>
  </xsl:template>
  
</xsl:transform>
