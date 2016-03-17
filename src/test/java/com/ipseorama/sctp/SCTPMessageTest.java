/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package com.ipseorama.sctp;

import com.ipseorama.sctp.messages.Chunk;
import com.ipseorama.sctp.messages.DataChunk;
import com.ipseorama.sctp.messages.SackChunk;
import com.ipseorama.sctp.messages.exceptions.SctpPacketFormatException;
import com.ipseorama.sctp.small.BlockingSCTPStream;
import java.io.IOException;
import java.util.TreeSet;
import org.bouncycastle.crypto.tls.DatagramTransport;
import org.junit.After;
import org.junit.AfterClass;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;
import static org.junit.Assert.*;

/**
 *
 * @author tim
 */
public class SCTPMessageTest {

    private SCTPStream _fakeStream;

    public SCTPMessageTest() {
    }

    @BeforeClass
    public static void setUpClass() {
    }

    @AfterClass
    public static void tearDownClass() {
    }
    Association _fakeAssociation;

    @Before
    public void setUp() {
        DatagramTransport _fakedt = new DatagramTransport() {

            @Override
            public int getReceiveLimit() throws IOException {
                return 1200;
            }

            @Override
            public int getSendLimit() throws IOException {
                return 1200;
            }

            @Override
            public int receive(byte[] bytes, int i, int i1, int waitMs) throws IOException {
                try {
                    Thread.sleep(waitMs);
                } catch (Exception x) {
                }
                throw new java.io.InterruptedIOException("empty");
            }

            @Override
            public void send(byte[] bytes, int i, int i1) throws IOException {
            }

            @Override
            public void close() throws IOException {
            }

        };
        _fakeAssociation = new Association(_fakedt, null) {
            @Override
            public void associate() throws SctpPacketFormatException, IOException {
                throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
            }

            @Override
            public void enqueue(DataChunk d) {
                throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
            }

            @Override
            public SCTPStream mkStream(int id) {
                throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
            }

            @Override
            public void sendAndBlock(SCTPMessage m) throws Exception {
                throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
            }

            @Override
            public SCTPMessage makeMessage(byte[] bytes, BlockingSCTPStream aThis) {
                throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
            }

            @Override
            public SCTPMessage makeMessage(String s, BlockingSCTPStream aThis) {
                throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
            }

            @Override
            protected Chunk[] sackDeal(SackChunk sackChunk) {
                throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
            }
        };
        _fakeStream = new SCTPStream(_fakeAssociation, new Integer(22)) {
            @Override
            public void send(String message) throws Exception {
                throw new UnsupportedOperationException("Not supported yet."); //To change body of generated methods, choose Tools | Templates.
            }

        };
    }

    @After
    public void tearDown() {
    }

    /**
     * Test of setCompleteHandler method, of class SCTPMessage.
     */
    /*
     @Test
     public void testSetCompleteHandler() {
     System.out.println("setCompleteHandler");
     MessageCompleteHandler mch = null;
     SCTPMessage instance = null;
     instance.setCompleteHandler(mch);
     // TODO review the generated test code and remove the default call to fail.
     fail("The test case is a prototype.");
     }
     */
    /**
     * Test of hasMoreData method, of class SCTPMessage.
     */
    /*
     @Test
     public void testHasMoreData() {

     }
     */
    /**
     * Test of fill method, of class SCTPMessage.
     */
    @Test
    public void testFillShortString() {
        System.out.println("--> fill short");
        String testString = "This is a short test";
        SCTPMessage instance = new SCTPMessage(testString, _fakeStream);
        TreeSet<DataChunk> chunks = new TreeSet<DataChunk>();
        while (instance.hasMoreData()) {
            DataChunk dc = new DataChunk();
            instance.fill(dc);
            chunks.add(dc);
        }
        assertEquals(chunks.size(), 1);
    }

    @Test
    public void testFillLongString() {
        System.out.println("--> fill long");
        StringBuffer sb = new StringBuffer("This is a");
        for (int i = 0; i < 1030; i++) {
            sb.append(" long");
        }
        sb.append(" test.");
        String testString = sb.toString();
        SCTPMessage instance = new SCTPMessage(testString, _fakeStream);
        TreeSet<DataChunk> chunks = new TreeSet<DataChunk>();
        long tsn = 111;

        while (instance.hasMoreData()) {
            DataChunk dc = new DataChunk();
            dc.setTsn(tsn++);
            instance.fill(dc);
            chunks.add(dc);
        }
        double pktsz = chunks.first().getDataSize();
        int estimate = (int) Math.ceil(testString.length() / pktsz);
        assertEquals(chunks.size(), estimate);
    }

    /**
     * Test of getData method, of class SCTPMessage.
     */
    /*
     @Test
     public void testGetData() {
     System.out.println("getData");
     SCTPMessage instance = null;
     byte[] expResult = null;
     byte[] result = instance.getData();
     assertArrayEquals(expResult, result);
     // TODO review the generated test code and remove the default call to fail.
     fail("The test case is a prototype.");
     }*/
}
