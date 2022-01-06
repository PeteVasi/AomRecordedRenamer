using System;
using System.IO;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace AOMRecordedRenamer
{
	public class AOMRCXParser
	{
    public String m_sXML;
    public DateTime m_timeGameStartTime;
    public int m_nPlayers;
    public int m_nTeams;
    public String m_sMap;
    public String m_sMode;
    public String m_sShortMode;
    public String[] m_aPlayerNames;
    public float[] m_aPlayerRatings;
    public String[] m_aPlayerCivs;
    public String[] m_aPlayerShortCivs;
    public int[] m_aPlayerTeams;
    public int[] m_aTeams;
    public String m_sTeamVsText;
    public int m_nCurrentPlayer;
    public bool m_bAgainstComputer;
    public String m_sGarbage;

		public AOMRCXParser()
		{
      Reset();
		}

    public void Reset()
    {
      m_sXML = "";
      m_timeGameStartTime = DateTime.FromFileTime(0);
      m_nPlayers = 0;
      m_nTeams = 0;
      m_sMap = "";
      m_sMode = "";
      m_sShortMode = "";
      m_sTeamVsText = "";
      m_aPlayerNames = null;
      m_aPlayerRatings = null;
      m_aPlayerCivs = null;
      m_aPlayerShortCivs = null;
      m_aTeams = null;
      m_nCurrentPlayer = -1;
      m_bAgainstComputer = false;
      m_sGarbage = "";
      m_aPlayerNames = new String[12];
      m_aPlayerRatings = new float[12];
      m_aPlayerCivs = new String[12];
      m_aPlayerShortCivs = new String[12];
      m_aPlayerTeams = new int[12];
      m_aTeams = new int[12];
      for(int i=0; i<12; i++)
      {
        m_aPlayerNames[i] = "";
        m_aPlayerRatings[i] = 0.0F;
        m_aPlayerCivs[i] = "";
        m_aPlayerShortCivs[i] = "";
        m_aPlayerTeams[i] = 0;
        m_aTeams[i] = 0;
      }
    }

    public bool Parse(String sFile, long nFileSize, DateTime dtFileCreationTime)
    {
      Reset();
      
      if(nFileSize <= 0)
        return false;

      m_timeGameStartTime = dtFileCreationTime;

      FileStream oFile = File.OpenRead(sFile);
      byte[] aHeader = new byte[4];
      if(oFile.Read(aHeader, 0, 4) == 4)
      {
        if(aHeader[0] == 'l' &&
           aHeader[1] == '3' &&
           aHeader[2] == '3' &&
           aHeader[3] == 't')
        {
          byte[] aXMLSize = new byte[4];
          if(oFile.Read(aXMLSize, 0, 4) == 4)
          {
            int nXMLSize = ((int)(aXMLSize[0])) +  //get original file size
                           (((int)aXMLSize[1]) << 8) +
                           (((int)aXMLSize[2]) << 16) +
                           (((int)aXMLSize[3]) << 24);
            byte[] aGzData = new byte[(((int)nFileSize)-8)];
            if((((int)nFileSize)-8) == oFile.Read(aGzData, 0, ((int)nFileSize)-8))
            {
              Inflater oDecompress = new Inflater(false);
              oDecompress.SetInput(aGzData, 0, (((int)nFileSize)-8));
              byte[] aXMLData = new byte[nXMLSize+1];
              if(oDecompress.Inflate(aXMLData, 0, nXMLSize) == nXMLSize)
              {
                System.Text.UnicodeEncoding encoding = new System.Text.UnicodeEncoding();
                System.Text.ASCIIEncoding ascii_encoding = new System.Text.ASCIIEncoding();
                int i, j, nStart=-1, nStop=-1;
                int[] aTeamMapper = new int[12];
                for(i=0; i<12; i++)
                  aTeamMapper[i] = -1;

                // Scroll through and determine "real" teams first
                for(i=0; i<nXMLSize-7; i++)
                {
                  m_sXML = ascii_encoding.GetString(aXMLData, i, 6);
                  if(m_sXML == "Team #")
                  {
                    int nCurTeamID = Convert.ToInt32(aXMLData[i+6]) - '1';
                    for(j=0; j<Convert.ToInt32(aXMLData[i+7]); j++)
                    { 
                      int nCurPlayerID = Convert.ToInt32(aXMLData[i+7+(4*(j+1))])-1;
                      if(nCurPlayerID >= 0  &&  nCurPlayerID < 12)
                        aTeamMapper[nCurPlayerID] = nCurTeamID;
                    }
                  }
                }

                // Get the actual XML data now
                for(i=0; i<nXMLSize-29; i++)
                {
                  m_sXML = encoding.GetString(aXMLData, i, 28);
                  if(m_sXML == "<GameSettings>")
                  {
                    nStart = i;
                    break;
                  }
                }
                if(nStart > 0)
                {
                  for(i=nStart+28; i<(nXMLSize-31); i++)
                  {
                    m_sXML = encoding.GetString(aXMLData, i, 30);
                    if(m_sXML == "</GameSettings>")
                    {
                      nStop = i;
                      break;
                    }
                  }
                  if(nStop > 0)
                  {
                    int nLen = nStop-nStart+30;
                    m_sXML = "";
                    bool bFirst = true;
                    while(nLen > 0)
                    {
                      if(nLen <= 0x404)
                      {
                        m_sXML += encoding.GetString(aXMLData, nStart, nLen);
                        nStart += 0x404;
                        nLen -= 0x404;
                      }
                      else if(bFirst)
                      {
                        m_sXML += encoding.GetString(aXMLData, nStart, 0x400-2);
                        nStart += 0x402;
                        nLen -= 0x402;
                        bFirst = false;
                      }
                      else
                      {
                        m_sXML += encoding.GetString(aXMLData, nStart, 0x400);
                        nStart += 0x404;
                        nLen -= 0x404;
                      }
                    }

                    //Create the XmlNamespaceManager.
                    NameTable nt = new NameTable();
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
                    nsmgr.AddNamespace("aom", "urn:aomx");

                    //Create the XmlParserContext.
                    XmlParserContext context = new XmlParserContext(null, nsmgr, null, XmlSpace.None);

                    //Create the reader. 
                    XmlTextReader xmlReader = new XmlTextReader(m_sXML, XmlNodeType.Element, context);
  
                    //Parse the XML.
                    int nPlayer = -1;
                    while(xmlReader.Read())
                    {
                      if(xmlReader.IsStartElement())
                      {
                        if(!xmlReader.IsEmptyElement)
                        {
                          String sName = xmlReader.LocalName;
                          String str = "";
                          switch(sName)
                          {
                            case "GameStartTime":
                              xmlReader.Read();
                              DateTime dtTemp;
                              dtTemp = GetDateTimeFromUnixEpoch(Convert.ToInt64(xmlReader.ReadString()));
                              if(dtTemp.Year >= 2002)
                                m_timeGameStartTime = dtTemp;
                              break;
                            case "Filename":
                              xmlReader.Read();
                              if(m_sMap == "")
                                m_sMap = xmlReader.ReadString();
                              break;
                            case "ScenarioFilename":
                              xmlReader.Read();
                              if(m_sMap == "")
                                m_sMap = xmlReader.ReadString();
                              break;
                            case "GameMode":
                              xmlReader.Read();
                              str = xmlReader.ReadString();
                              m_sMode = GetModeNameFromInt(Convert.ToInt32(str));
                              m_sShortMode = GetShortModeNameFromInt(Convert.ToInt32(str));
                              break;                              
                            case "NumPlayers":
                              xmlReader.Read();
                              m_nPlayers = Convert.ToInt32(xmlReader.ReadString());
                              break;
                            case "CurrentPlayer":
                              xmlReader.Read();
                              m_nCurrentPlayer = Convert.ToInt32(xmlReader.ReadString())-1;
                              break;
                            case "Player":
                              nPlayer++;
                              break;
                            case "Name":
                              xmlReader.Read();
                              m_aPlayerNames[nPlayer] = xmlReader.ReadString();
                              break;
                            case "Rating":
                              xmlReader.Read();
                              str = xmlReader.ReadString();
                              str = str.Replace(',', '.');
                              m_aPlayerRatings[nPlayer] = Convert.ToSingle(str);
                              break;
                            case "Civilization":
                              xmlReader.Read();
                              str = xmlReader.ReadString();
                              m_aPlayerCivs[nPlayer] = GetCivNameFromInt(Convert.ToInt32(str));
                              m_aPlayerShortCivs[nPlayer] = GetShortCivNameFromInt(Convert.ToInt32(str));
                              break;
                            case "Team":
                              xmlReader.Read();
                              m_aPlayerTeams[nPlayer] = Convert.ToInt32(xmlReader.ReadString());
                              break;
                            case "Type":
                              xmlReader.Read();
                              int nType = Convert.ToInt32(xmlReader.ReadString());
                              if(nType != 0)
                                m_bAgainstComputer = true;
                              else if(m_nCurrentPlayer < 0)
                                m_nCurrentPlayer = nPlayer;
                              break;
                            default:
                              break;
                          }
                        }
                      }
                    }
  
                    //Close the reader.
                    xmlReader.Close();

                    // Scroll and grab some "garbage"
                    /*
                    for(i=0; i<m_nPlayers; i++)
                    {
                      String sCompare = "";
                      for(j=0; j<nXMLSize-64; j++)
                      {
                        sCompare = encoding.GetString(aXMLData, j, this.m_aPlayerNames[i].Length*2);
                        if(sCompare == this.m_aPlayerNames[i])
                        {
                          // TODO: Parse some garbage
                        }
                      }
                    }
                    */

                    // Calculate players per team, and team vs text
                    for(i=0; i<m_nPlayers; i++)
                    {
                      if(aTeamMapper[i] >= 0  &&  aTeamMapper[i] < 12)
                      {
                        m_aPlayerTeams[i] = aTeamMapper[i]+1;
                        m_aTeams[m_aPlayerTeams[i]-1]++;
                      }
                    }
                    bFirst = true;
                    for(i=0; i<12; i++)
                    {
                      if(m_aTeams[i] > 0)
                      {
                        m_nTeams++;
                        if(bFirst)
                          m_sTeamVsText = Convert.ToString(m_aTeams[i]);
                        else
                          m_sTeamVsText += "v" + Convert.ToString(m_aTeams[i]);
                        bFirst = false;
                      }
                    }
                    if(m_nCurrentPlayer < 0)
                      m_nCurrentPlayer = 0;

                    oFile.Close();
                    return true;
                  }
                }
              }
            }
          }
        }
      }

      oFile.Close();
      return false;
    }

    public String GetCivNameFromInt(int nCiv)
    {
      switch(nCiv)
      {
        case 0:   return "Zeus";
        case 1:   return "Poseidon";
        case 2:   return "Hades";
        case 3:   return "Isis";
        case 4:   return "Ra";
        case 5:   return "Set";
        case 6:   return "Odin";
        case 7:   return "Thor";
        case 8:   return "Loki";
        case 9:   return "Kronos";// : "Random All";
        case 10:  return "Oranos";// : "Random Greek";
        case 11:  return "Gaia";// : "Random Norse";
        case 12:  return "Random All";// : "Random Egyptian";
        case 13:  return "Random Greek";
        case 14:  return "Random Norse";
        case 15:  return "Random Egyptian";
        case 16:  return "Random Atlantean";
        default:  return "Civ Unknown";
      }
    }

    public String GetShortCivNameFromInt(int nCiv)
    {
      switch(nCiv)
      {
        case 0:   return "Zu";
        case 1:   return "Po";
        case 2:   return "Ha";
        case 3:   return "Is";
        case 4:   return "Ra";
        case 5:   return "St";
        case 6:   return "Od";
        case 7:   return "Th";
        case 8:   return "Lo";
        case 9:   return "Kr";// : "Random All";
        case 10:  return "Or";// : "Random Greek";
        case 11:  return "Ga";// : "Random Norse";
        case 12:  return "Rnd";// : "Random Egyptian";
        case 13:  return "RnG";
        case 14:  return "RnN";
        case 15:  return "RnE";
        case 16:  return "RnA";
        default:  return "Unk";
      }
    }

    public String GetModeNameFromInt(int nMode)
    {
      switch(nMode)
      {
        case 0:   return "Supremacy";
        case 1:   return "Conquest";
        case 2:   return "Lightning";
        case 3:   return "Deathmatch";
        default:  return "Unknown Mode";
      }
    }

    public String GetShortModeNameFromInt(int nMode)
    {
      switch(nMode)
      {
        case 0:   return "Sup";
        case 1:   return "Con";
        case 2:   return "Lit";
        case 3:   return "DM";
        default:  return "Unk";
      }
    }

    public long GetEpochTime()
    {
      // Mostly copied from: http://weblogs.asp.net/brada/archive/2004/03/20/93332.aspx
      DateTime dtCurTime = DateTime.Now;
      DateTime dtEpochStartTime = Convert.ToDateTime("1/1/1970 12:00:00 AM").ToLocalTime();
      TimeSpan ts = dtCurTime.Subtract(dtEpochStartTime);
      long epochtime;
      epochtime = ((((((ts.Days * 24) + ts.Hours) * 60) + ts.Minutes) * 60) + ts.Seconds);
      return epochtime;
    }

    public DateTime GetDateTimeFromUnixEpoch(long nTimestamp) 
    {
      DateTime dt = Convert.ToDateTime("1/1/1970 12:00:00 AM").ToLocalTime();
      dt = dt.AddSeconds(nTimestamp);
      return dt;
    }

	}
}
